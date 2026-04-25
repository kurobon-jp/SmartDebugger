#if ENABLE_INSTANTREPLAY && !EXCLUDE_INSTANTREPLAY
#define USE_INSTANTREPLAY
#endif
using System.Collections;
using System.Text;
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

        protected override void OnEnable()
        {
            base.OnEnable();
            _bugReporter = SDSettings.Instance.BugReporter;
            _includeLogs.SetIsOnWithoutNotify(_bugReporter.IncludeLogs);
            _includeScreenCapture.SetIsOnWithoutNotify(_bugReporter.IncludeScreenCapture);
#if USE_INSTANTREPLAY
            _includeScreenRecord.gameObject.SetActive(_bugReporter is ScreenRecordBugReporter);
            _includeScreenRecord.SetIsOnWithoutNotify(_bugReporter.IncludeScreenRecord);
#else
            _includeScreenRecord.gameObject.SetActive(false);
#endif
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
#if USE_INSTANTREPLAY
                || (_includeScreenRecord.isOn && _includeScreenRecord.gameObject.activeSelf)
#endif
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
            _buttonText.text = "Sending...";
            _sendButton.interactable = false;
            _eventSystem = EventSystem.current;
            if (_eventSystem != null)
                _eventSystem.enabled = false;

            var description = _description.text;
            var report = "";
            byte[] screenShot = null;
            string videoPath = null;
            if (_includeLogs.isOn)
            {
                report = CreateReport(description);
            }

            if (_includeScreenCapture.isOn)
            {
                screenShot = await TakeScreenShotAsync();
            }
#if USE_INSTANTREPLAY
            if (_includeScreenRecord.isOn && _bugReporter is ScreenRecordBugReporter screenRecordBugReporter)
            {
                videoPath = await screenRecordBugReporter.StopRecordingAndExportAsync();
            }
#endif
            _bugReporter.SendReport(description, report, screenShot, videoPath, OnResult);
        }

        private async Task<byte[]> TakeScreenShotAsync()
        {
            var tcs = new TaskCompletionSource<byte[]>();
            StartCoroutine(TakeScreenShotCoroutine(tcs));
            return await tcs.Task;
        }

        private static IEnumerator TakeScreenShotCoroutine(TaskCompletionSource<byte[]> tcs)
        {
            SmartDebug.Instance.HideMenu();
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            SmartDebug.Instance.OpenMenu();
            var screenShot = tex.EncodeToPNG();
            Destroy(tex);
            tcs.SetResult(screenShot);
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
            _isSending = false;
            _buttonText.text = _bugReporter.SendTo;
            _sendButton.interactable = true;
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

            if (_eventSystem != null)
                _eventSystem.enabled = true;
        }
    }
}