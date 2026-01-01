using System.Collections;
using System.Text;
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
        [SerializeField] private Toggle _includeScreenshot;
        [SerializeField] private Toggle _includeLogs;

        private BugReporter _bugReporter;
        private EventSystem _eventSystem;
        private bool _isSending;

        protected override void OnEnable()
        {
            base.OnEnable();
            _bugReporter = SDSettings.Instance.BugReporter;
            _buttonText.text = _bugReporter.SendTo;
            _openButton.gameObject.SetActive(false);
            OnValueChanged();
        }

        public void OnValueChanged()
        {
            if (_isSending) return;
            _sendButton.interactable =
                !string.IsNullOrEmpty(_description.text) || _includeScreenshot.isOn || _includeLogs.isOn;
        }

        public void SendReport()
        {
            if (_isSending) return;
            _isSending = true;
            _openButton.gameObject.SetActive(false);
            SmartDebug.Instance.StartCoroutine(SendReportCoroutine());
        }

        private IEnumerator SendReportCoroutine()
        {
            var description = _description.text;
            byte[] screenShot = null;
            var report = "";
            if (_includeScreenshot.isOn)
            {
                SmartDebug.Instance.CloseMenu();
                yield return new WaitForEndOfFrame();
                var tex = ScreenCapture.CaptureScreenshotAsTexture();
                SmartDebug.Instance.OpenMenu();
                screenShot = tex.EncodeToPNG();
                Destroy(tex);
            }

            if (_includeLogs.isOn)
            {
                report = CreateReport(description);
            }

            _buttonText.text = "Sending...";
            _sendButton.interactable = false;
            _eventSystem = EventSystem.current;
            if (_eventSystem != null)
                _eventSystem.enabled = false;
            _bugReporter.SendReport(description, report, screenShot, OnResult);
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
                sb.AppendLine($"- {entry.Types}:{entry.Message}");
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
            if (result.IsSuccess && result.ReportUris is { Length: > 0 })
            {
                _openButton.gameObject.SetActive(true);
                _openButton.onClick.RemoveAllListeners();
                _openButton.onClick.AddListener(() =>
                {
                    foreach (var uri in result.ReportUris)
                    {
                        Application.OpenURL(uri.ToString());
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