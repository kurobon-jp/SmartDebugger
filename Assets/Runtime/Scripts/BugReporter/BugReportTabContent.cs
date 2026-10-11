using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class BugReportTabContent : MainTabContent
    {
        [SerializeField] private InputField _description;
        [SerializeField] private Button _sendButton;
        [SerializeField] private Button _openButton;
        [SerializeField] private Text _buttonText;
        [SerializeField] private Toggle _includeLogs;
        [SerializeField] private Toggle _includeScreenCapture;
        [SerializeField] private Toggle _includeScreenRecord;

        private BugReporter _bugReporter;
        private EventSystem _eventSystem;
        private bool _isSending;
        private bool _eventSystemWasEnabled;
        private CancellationTokenSource _sendCancellation;

        protected override void OnDisable()
        {
            _sendCancellation?.Cancel();
            RestoreEventSystem();
            base.OnDisable();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _bugReporter = SDSettings.Instance.BugReporter;
            _includeLogs.SetIsOnWithoutNotify(_bugReporter.IncludeLogs);
            _includeScreenCapture.SetIsOnWithoutNotify(_bugReporter.IncludeScreenCapture);
            _includeScreenRecord.gameObject.SetActive(
                _bugReporter is ScreenRecordBugReporter { IsScreenRecordingAvailable: true });
            _includeScreenRecord.SetIsOnWithoutNotify(_bugReporter.IncludeScreenRecord);
            _buttonText.text = _bugReporter.SendTo;
            _openButton.gameObject.SetActive(false);
            OnValueChanged();
        }

        public void OnValueChanged()
        {
            if (_isSending) return;
            _bugReporter.IncludeLogs.Value = _includeLogs.isOn;
            _bugReporter.IncludeScreenCapture.Value = _includeScreenCapture.isOn;
            _bugReporter.IncludeScreenRecord.Value = _includeScreenRecord.isOn;
            _sendButton.interactable =
                !string.IsNullOrEmpty(_description.text)
                || (_includeLogs.isOn && _includeLogs.gameObject.activeSelf)
                || (_includeScreenCapture.isOn && _includeScreenCapture.gameObject.activeSelf)
                || (_includeScreenRecord.isOn && _includeScreenRecord.gameObject.activeSelf)
                ;
        }

        public void SendReport()
        {
            if (_isSending) return;
            _isSending = true;
            _openButton.gameObject.SetActive(false);
            SendReportAsync();
        }

        private async void SendReportAsync()
        {
            var cancellation = _sendCancellation = new CancellationTokenSource();
            try
            {
                _buttonText.text = "Saving...";
                _sendButton.interactable = false;
                _eventSystem = EventSystem.current;
                _eventSystemWasEnabled = _eventSystem != null && _eventSystem.enabled;
                if (_eventSystem != null) _eventSystem.enabled = false;

                var description = _description.text;
                var report = "";
                byte[] screenShot = null;
                string videoPath = null;
                if (_includeLogs.isOn) report = CreateReport(description);
                if (_includeScreenCapture.isOn)
                    screenShot = await TakeScreenShotAsync(cancellation.Token);
                if (_includeScreenRecord.isOn && _includeScreenRecord.gameObject.activeSelf
                    && _bugReporter is ScreenRecordBugReporter screenRecordBugReporter)
                {
                    videoPath = await BugReportSender.WaitAsync(screenRecordBugReporter.StopRecordingAndExportAsync(),
                        cancellation.Token, TimeSpan.FromSeconds(30));
                }

                cancellation.Token.ThrowIfCancellationRequested();
                var result = await BugReportSender.SendAsync(_bugReporter, description, report, screenShot, videoPath,
                    cancellation.Token, TimeSpan.FromSeconds(60));
                if (this != null && !cancellation.IsCancellationRequested) OnResult(result);
            }
            catch (OperationCanceledException)
            {
                // Closing the tab cancels the UI wait; late reporter callbacks are ignored.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (this != null) OnResult(ReportResult.Failed);
            }
            finally
            {
                RestoreEventSystem();
                _isSending = false;
                _sendCancellation = null;
                if (_buttonText != null && _bugReporter != null) _buttonText.text = _bugReporter.SendTo;
                if (this != null && _sendButton != null) OnValueChanged();
                cancellation.Dispose();
            }
        }

        private async Task<byte[]> TakeScreenShotAsync(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<byte[]>();
            var debugger = SmartDebug.Instance;
            var coroutine = StartCoroutine(TakeScreenShotCoroutine(tcs, debugger));
            using var registration = cancellationToken.Register(() => tcs.TrySetCanceled());
            try
            {
                return await BugReportSender.WaitAsync(tcs.Task, cancellationToken, TimeSpan.FromSeconds(10));
            }
            finally
            {
                if (this != null) StopCoroutine(coroutine);
                if (debugger != null) debugger.ShowCanvas();
            }
        }

        private static IEnumerator TakeScreenShotCoroutine(TaskCompletionSource<byte[]> tcs, SmartDebug debugger)
        {
            debugger.HideCanvas();
            yield return new WaitForEndOfFrame();
            Texture2D tex = null;
            try
            {
                tex = ScreenCapture.CaptureScreenshotAsTexture();
                tcs.TrySetResult(tex.EncodeToPNG());
            }
            catch (Exception exception)
            {
                tcs.TrySetException(exception);
            }
            finally
            {
                if (tex != null) Destroy(tex);
                if (debugger != null) debugger.ShowCanvas();
            }
        }

        private string CreateReport(string description)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(description))
            {
                sb.AppendLine("--------------Description-------------------");
                sb.AppendLine(description);
                sb.AppendLine();
            }

            sb.AppendLine("--------------SystemInfo-------------------");
            var systemInfos = SystemInfo.GetSystemInfoText();
            foreach (var systemInfo in systemInfos)
            {
                sb.AppendLine($"- {systemInfo.Key}");
                sb.AppendLine($"{systemInfo.Value}");
                sb.AppendLine();
            }

            sb.AppendLine("--------------Logs-------------------");
            var entries = SmartDebug.Instance.LogReceiver.Entries;
            foreach (var entry in entries.Values)
            {
                sb.AppendLine($"- [{entry.Time:HH:mm:ss}] {entry.Types}: {entry.Message}");
                foreach (var stackTrace in entry.StackTrace)
                {
                    sb.AppendLine(stackTrace);
                }
            }

            return sb.ToString();
        }

        private void OnResult(ReportResult result)
        {
            if (result.IsSuccess && result.ReportUrls is { Length: > 0 })
            {
                _openButton.gameObject.SetActive(true);
                _openButton.onClick.RemoveAllListeners();
                _openButton.onClick.AddListener(() =>
                {
                    foreach (var url in result.ReportUrls)
                    {
                        Application.OpenURL(url);
                    }
                });
            }
            else
            {
                _openButton.gameObject.SetActive(false);
            }

        }

        private void RestoreEventSystem()
        {
            if (_eventSystem != null) _eventSystem.enabled = _eventSystemWasEnabled;
            _eventSystem = null;
        }
    }
}
