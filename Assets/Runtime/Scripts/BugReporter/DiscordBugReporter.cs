using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using UnityEngine;

namespace SmartDebugger
{
    [CreateAssetMenu(fileName = "DiscordBugReporter", menuName = "SmartDebugger/BugReporter/DiscordBugReporter")]
    public class DiscordBugReporter : ScreenRecordBugReporter
    {
        private static readonly HttpClient Client = new();

        [SerializeField] private string _serverId;
        [SerializeField] private string _webhookUrl;

        public override string SendTo => "Send to Discord";

        public override void SendReport(string description, string report, byte[] screenshot, string videoPath,
            Action<ReportResult> onResult)
        {
            SendReportAsync(description, report, screenshot, videoPath, onResult);
        }

        private async void SendReportAsync(string description, string report, byte[] screenshot, string videoPath,
            Action<ReportResult> onResult)
        {
            if (string.IsNullOrEmpty(_webhookUrl))
            {
                Debug.LogError("Discord Webhook URL is not set.");
                onResult(ReportResult.Failed);
                return;
            }

            try
            {
                using var form = new MultipartFormDataContent();
                var filePrefix = CreateFilePrefix();
                form.Add(new StringContent(description), "content");

                var fileIndex = 0;
                // --- レポート（テキストファイル） ---
                if (!string.IsNullOrEmpty(report))
                {
                    var content = new ByteArrayContent(Encoding.UTF8.GetBytes(report));
                    form.Add(content, $"file{fileIndex++}", $"{filePrefix}_report.txt");
                }

                // --- スクリーンショット（画像ファイル） ---
                if (screenshot is { Length: > 0 })
                {
                    var content = new ByteArrayContent(screenshot);
                    form.Add(content, $"file{fileIndex++}", $"{filePrefix}_screenshot.png");
                }

                // --- スクリーンショット（画像ファイル） ---
                if (!string.IsNullOrEmpty(videoPath) && File.Exists(videoPath))
                {
                    var content = new ByteArrayContent(File.ReadAllBytes(videoPath));
                    form.Add(content, $"file{fileIndex}", $"{filePrefix}_screen_record.mp4");
                }
                
                // 一度メモリに書き出す
                var ms = new MemoryStream();
                await form.CopyToAsync(ms);
                ms.Position = 0;

                // GZip圧縮
                using var compressedStream = new MemoryStream();
                await using (var gzip = new GZipStream(compressedStream, CompressionMode.Compress, true))
                {
                    await ms.CopyToAsync(gzip);
                }
                compressedStream.Position = 0;

                // HttpContentにする
                using var compressedContent = new StreamContent(compressedStream);
                compressedContent.Headers.ContentEncoding.Add("gzip");
                compressedContent.Headers.ContentType = form.Headers.ContentType;

                // Discordへ送信
                var response = await Client.PostAsync(_webhookUrl, form);
                if (response.IsSuccessStatusCode)
                {
                    Debug.Log("Discord report sent successfully.");
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonUtility.FromJson<ApiResult>(json);
                    if (result != null)
                    {
                        var url = $"https://discord.com/channels/{_serverId}/{result.channel_id}/{result.id}";
                        onResult(ReportResult.Success(url));
                    }
                    else
                    {
                        onResult(ReportResult.Success());
                    }
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Discord API Error: {response.StatusCode} - {errorMsg}");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                onResult(ReportResult.Failed);
            }
        }

        private class ApiResult
        {
            public string id;
            public string channel_id;
        }
    }
}