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

        private readonly List<IEventDetector> _openEventDetectors = new();
        private readonly List<IEventDetector> _closeEventDetectors = new();
        private readonly List<IFieldLayout> _fieldLayouts = new();

        public LogReceiver LogReceiver { get; private set; }
        internal FrameRecorder FrameRecorder { get; private set; }

        internal IReadOnlyList<IFieldLayout> FieldLayouts => _fieldLayouts;

        public static SmartDebug Instance
        {
            get
            {
                Initialize();
                return _instance;
            }
        }

        public bool IsCanvasVisible => _canvas.gameObject.activeSelf;

        public event Action<bool> OnCanvasVisiblityChanged;

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
            LogReceiver = new LogReceiver();
            FrameRecorder = new FrameRecorder();
            if (SDSettings.Instance.IsAutoGenerateEventSystem && EventSystem.current == null)
            {
                var go = new GameObject("EventSystem");
                go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                go.AddComponent<StandaloneInputModule>();
#endif
            }

            var openShortcut = SDSettings.Instance.OpenShortcut;
            if (openShortcut.IsEnable)
            {
                _openEventDetectors.Add(EventDetectorFactory.CreateKeyEventDetector(openShortcut));
            }

            var closeShortcut = SDSettings.Instance.CloseShortcut;
            if (closeShortcut.IsEnable)
            {
                _closeEventDetectors.Add(EventDetectorFactory.CreateKeyEventDetector(closeShortcut));
            }

            var openTapEvent = SDSettings.Instance.OpenTapEvent;
            if (openTapEvent.IsEnable)
            {
                _openEventDetectors.Add(EventDetectorFactory.CreateMultiTapEventDetector(openTapEvent));
            }
        }

        private void OnDestroy()
        {
            FrameRecorder?.Dispose();
            FrameRecorder = null;
        }

        public void OpenCanvas(bool showLog = false)
        {
            if (IsCanvasVisible) return;
            if (showLog)
            {
                _canvas.Open<LogTabContent>();
            }
            else
            {
                _canvas.Open();
            }

            OnCanvasVisiblityChanged?.Invoke(true);
        }

        public void CloseCanvas()
        {
            if (!IsCanvasVisible) return;
            _canvas.Close();
            OnCanvasVisiblityChanged?.Invoke(false);
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
            FrameRecorder?.Sample();
            if (!IsCanvasVisible)
            {
                foreach (var detector in _openEventDetectors)
                {
                    if (!detector.IsTriggered()) continue;
                    OpenCanvas(ErrorIndicator.HasError);
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