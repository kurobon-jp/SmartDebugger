using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SmartDebugger
{
    public class SmartDebug : MonoBehaviour
    {
        private static SmartDebug _instance;

        [SerializeField] private SDCanvas _canvas;
        [SerializeField] private ErrorIndicator _indicator;
        
        private readonly List<IEventDetector> _openEventDetectors = new();
        private readonly List<IEventDetector> _closeEventDetectors = new();
        private readonly FieldLayouts _fieldLayouts = new();

        public LogReceiver LogReceiver { get; private set; }
        internal FrameRecorder FrameRecorder { get; private set; }

        internal FieldLayouts FieldLayouts => _fieldLayouts;

        public static SmartDebug Instance
        {
            get
            {
                Initialize();
                return _instance;
            }
        }

        public bool IsCanvasVisible => _canvas.gameObject.activeSelf;

        public event Action<bool> OnCanvasVisibilityChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnSceneLoaded()
        {
            if (!Application.isPlaying || !SDSettings.Instance.IsAutoInitialize) return;
            Initialize();
        }

        public static void Initialize()
        {
            if (_instance != null) return;
            var prefab = SDSettings.Instance.LoadPrefab<SmartDebug>("SmartDebug");
            _instance = Instantiate(prefab);
            if (SDSettings.Instance.IsDontDestroyOnLoad)
            {
                DontDestroyOnLoad(_instance.gameObject);
            }

            SDSettings.Instance.BugReporter?.Initialize();
        }

        private void Awake()
        {
            var settings = SDSettings.Instance;
            LogReceiver = new LogReceiver();
            FrameRecorder = new FrameRecorder();
            if (settings.IsAutoGenerateEventSystem && EventSystem.current == null)
            {
                var go = new GameObject("EventSystem");
                go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                go.AddComponent<StandaloneInputModule>();
#endif
            }

            var openShortcut = settings.OpenShortcut;
            if (openShortcut.IsEnable)
            {
                _openEventDetectors.Add(EventDetectorFactory.CreateKeyEventDetector(openShortcut));
            }

            var closeShortcut = settings.CloseShortcut;
            if (closeShortcut.IsEnable)
            {
                _closeEventDetectors.Add(EventDetectorFactory.CreateKeyEventDetector(closeShortcut));
            }

            var openTapEvent = settings.OpenTapEvent;
            if (openTapEvent.IsEnable)
            {
                _openEventDetectors.Add(EventDetectorFactory.CreateMultiTapEventDetector(openTapEvent));
            }

            LogReceiver.OnAdded += OnLogAdded;
        }

        private void OnLogAdded(LogEntry entry)
        {
            if (entry.Types != LogTypes.Error || !SDSettings.Instance.IsShowErrorIndicator || IsCanvasVisible) return;
            _indicator.Blink();
        }

        private void OnDestroy()
        {
            FrameRecorder?.Dispose();
            FrameRecorder = null;
            LogReceiver.OnAdded -= OnLogAdded;
        }

        public void OpenCanvas()
        {
            if (IsCanvasVisible) return;
            _canvas.Open();
            OnCanvasVisibilityChanged?.Invoke(true);
        }

        public void CloseCanvas()
        {
            if (!IsCanvasVisible) return;
            _canvas.Close();
            OnCanvasVisibilityChanged?.Invoke(false);
        }

        internal void ShowCanvas()
        {
            _canvas.SetAlpha(1f);
        }

        internal void HideCanvas()
        {
            _canvas.SetAlpha(0f);
        }

        private void LateUpdate()
        {
            LogReceiver?.Update();
            FrameRecorder?.Sample();
            if (!IsCanvasVisible)
            {
                foreach (var detector in _openEventDetectors)
                {
                    if (!detector.IsTriggered()) continue;
                    OpenCanvas();
                    break;
                }
            }
            else
            {
                foreach (var detector in _closeEventDetectors)
                {
                    if (!detector.IsTriggered()) continue;
                    CloseCanvas();
                    break;
                }
            }
        }

        public void AddFieldLayout(IFieldLayout fieldLayout)
        {
            _fieldLayouts.Add(fieldLayout);
        }

        public void RemoveFieldLayout(IFieldLayout fieldLayout)
        {
            _fieldLayouts.Remove(fieldLayout);
        }
    }
}