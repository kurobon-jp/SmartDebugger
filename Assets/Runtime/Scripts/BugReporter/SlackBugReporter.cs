using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SmartDebugger
{
    [CreateAssetMenu(fileName = "SlackBugReporter", menuName = "SmartDebugger/BugReporter/SlackBugReporter")]
    public class SlackBugReporter : BugReporter
    {
        private const string SlackApi = "https://slack.com/api";
        private static readonly string GetUploadURLExternal = $"{SlackApi}/files.getUploadURLExternal";
        private static readonly string CompleteUploadExternal = $"{SlackApi}/files.completeUploadExternal";
        private static readonly string PostMessage = $"{SlackApi}/chat.postMessage";

        private readonly HttpClient _client = new();

        [SerializeField] private string _channelId;
        [SerializeField] private string _token;

        public override string SendTo => "Send to Slack";

        private async void SendReportAsync(string description, string report, byte[] screenshot, Action<ReportResult> onResult)
        {
            if (string.IsNullOrEmpty(_token) || string.IsNullOrEmpty(_channelId))
            {
                Debug.LogError("Slack token or channel ID is not set.");
                onResult(ReportResult.Failed);
                return;
            }

            try
            {
                var filePrefix = CreateFilePrefix();
                var uploads = new List<FileUpload>();
                if (!string.IsNullOrEmpty(report))
                {
                    uploads.Add(
                        new FileUpload($"{filePrefix}_report.txt", Encoding.UTF8.GetBytes(report), "text/plain"));
                }

                if (screenshot is { Length: > 0 })
                {
                    uploads.Add(new FileUpload($"{filePrefix}_screenshot.png", screenshot, "image/png"));
                }

                if (uploads.Count > 0)
                {
                    await UploadAsync(description, uploads);
                }
                else
                {
                    await PostMessageAsync(description);
                }

                onResult(new ReportResult(true, $"https://app.slack.com/archives/{_channelId}"));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                onResult(ReportResult.Failed);
            }
        }

        private async Task UploadAsync(string message, List<FileUpload> uploads)
        {
            var tasks = new List<Task<string>>();
            foreach (var upload in uploads)
            {
                tasks.Add(UploadAsync(upload));
            }

            var fileIds = await Task.WhenAll(tasks);
            await CompleteUploadAsync(message, fileIds);
        }

        private async Task<GetUploadURLExternalResult> GetUploadUrlAsync(string filename, byte[] data)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(_token), "token");
            form.Add(new StringContent(filename), "filename");
            form.Add(new StringContent($"{data.Length}"), "length");
            var response = await _client.PostAsync(GetUploadURLExternal, form);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonUtility.FromJson<GetUploadURLExternalResult>(json);
            return result.ok ? result : throw new Exception($"Upload failed.\n{result.error}");
        }

        private async Task<string> UploadAsync(FileUpload upload)
        {
            var res = await GetUploadUrlAsync(upload.FileName, upload.Content);
            using var stream = new MemoryStream(upload.Content);
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(upload.ContentType);
            var response = await _client.PostAsync(res.upload_url, content);
            response.EnsureSuccessStatusCode();
            Debug.Log($"Upload file. {upload.FileName} {res.file_id}");
            return res.file_id;
        }

        private async Task CompleteUploadAsync(string description, string[] fileIds)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(_token), "token");
            form.Add(new StringContent(_channelId), "channel_id");
            form.Add(new StringContent(description), "initial_comment");
            var filesJson = "[" + string.Join(",", fileIds.Select(id => $"{{\"id\":\"{id}\"}}")) + "]";
            form.Add(new StringContent(filesJson), "files");
            var response = await _client.PostAsync(CompleteUploadExternal, form);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonUtility.FromJson<ApiResult>(json);
            if (result.ok)
            {
                Debug.Log("Upload completed.");
            }
            else
            {
                throw new Exception($"Upload failed.\n{result.error}");
            }
        }

        private async Task PostMessageAsync(string description)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(_token), "token");
            form.Add(new StringContent(_channelId), "channel");
            form.Add(new StringContent(description), "text");
            var response = await _client.PostAsync(PostMessage, form);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonUtility.FromJson<ApiResult>(json);
            if (result.ok)
            {
                Debug.Log("Post completed.");
            }
            else
            {
                throw new Exception($"Post failed.\n{result.error}");
            }
        }


        public override void SendReport(string description, string report, byte[] screenshot, Action<ReportResult> onResult)
        {
            SendReportAsync(description, report, screenshot, onResult);
        }

        private class FileUpload
        {
            public readonly string FileName;
            public readonly string ContentType;
            public readonly byte[] Content;

            public FileUpload(string fileName, byte[] content, string contentType)
            {
                FileName = fileName;
                ContentType = contentType;
                Content = content;
            }
        }

        private class GetUploadURLExternalResult : ApiResult
        {
            public string upload_url;
            public string file_id;
        }

        private class ApiResult
        {
            public bool ok;
            public string error;
        }
    }
}