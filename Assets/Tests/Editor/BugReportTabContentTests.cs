using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace SmartDebugger.Tests
{
    public class BugReportTabContentTests
    {
        private GameObject _root;
        private GameObject _eventRoot;
        private EventSystem _eventSystem;
        private List<EventSystem> _eventSystems;
        private EventSystem[] _previousEventSystems;
        private BugReportTabContent _view;
        private StubBugReporter _reporter;
        private UnityEngine.UI.Button _button;
        private readonly Dictionary<string, int?> _savedPreferences = new();

        private void Set(string field, object value) => typeof(BugReportTabContent)
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_view, value);

        private T Component<T>() where T : Component
        {
            var child = new GameObject(typeof(T).Name, typeof(RectTransform));
            child.transform.SetParent(_root.transform);
            var component = child.AddComponent<T>();
            if (component is UnityEngine.UI.Toggle toggle) toggle.isOn = false;
            return component;
        }

        [SetUp]
        public void SetUp()
        {
            foreach (var key in new[] { "sd.include_logs", "sd.include_screen_capture", "sd.include_screen_record" })
                _savedPreferences[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null;
            // EventSystem is not ExecuteAlways, so EditMode does not run its
            // registration callback. Register only for this test and restore it.
            _eventSystems = (List<EventSystem>)typeof(EventSystem)
                .GetField("m_EventSystems", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            _previousEventSystems = _eventSystems.ToArray();
            _eventRoot = new GameObject("Submission test EventSystem");
            _eventSystem = _eventRoot.AddComponent<EventSystem>();
            _eventSystems.Insert(0, _eventSystem);
            _root = new GameObject("Submission test view");
            _root.SetActive(false);
            _view = _root.AddComponent<BugReportTabContent>();
            _reporter = ScriptableObject.CreateInstance<StubBugReporter>();
            Set("_bugReporter", _reporter);
            var description = Component<UnityEngine.UI.InputField>();
            description.text = "Test report";
            Set("_description", description);
            _button = Component<UnityEngine.UI.Button>();
            Set("_sendButton", _button);
            Set("_openButton", Component<UnityEngine.UI.Button>());
            Set("_buttonText", Component<UnityEngine.UI.Text>());
            Set("_includeLogs", Component<UnityEngine.UI.Toggle>());
            Set("_includeScreenCapture", Component<UnityEngine.UI.Toggle>());
            Set("_includeScreenRecord", Component<UnityEngine.UI.Toggle>());
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_root);
            UnityEngine.Object.DestroyImmediate(_eventRoot);
            UnityEngine.Object.DestroyImmediate(_reporter);
            _eventSystems.Clear();
            _eventSystems.AddRange(_previousEventSystems);
            foreach (var preference in _savedPreferences)
            {
                if (preference.Value.HasValue) PlayerPrefs.SetInt(preference.Key, preference.Value.Value);
                else PlayerPrefs.DeleteKey(preference.Key);
            }
            _savedPreferences.Clear();
        }

        [UnityTest]
        public IEnumerator ReporterExceptionRestoresInputAndSendButton()
        {
            _reporter.Send = _ => throw new InvalidOperationException("submission test failure");
            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: submission test failure"));
            _view.SendReport();
            yield return null;
            Assert.That(_eventSystem.enabled, Is.True);
            Assert.That(_button.interactable, Is.True);
        }

        [UnityTest]
        public IEnumerator FailedReportPreservesDisabledEventSystem()
        {
            _eventSystem.enabled = false;
            _reporter.Send = callback => callback(ReportResult.Failed);
            _view.SendReport();
            yield return null;
            Assert.That(_eventSystem.enabled, Is.False);
            Assert.That(_button.interactable, Is.True);
        }

        [UnityTest]
        public IEnumerator ClosingTabRestoresInputAndIgnoresLateResult()
        {
            Action<ReportResult> callback = null;
            _reporter.Send = result => callback = result;
            _view.SendReport();
            Assert.That(_eventSystem.enabled, Is.False);
            typeof(BugReportTabContent).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_view, null);
            yield return null;
            yield return null;
            Assert.That(_eventSystem.enabled, Is.True);
            callback(ReportResult.Success("ignored"));
            Assert.That(_button.interactable, Is.True);
        }
    }
}
