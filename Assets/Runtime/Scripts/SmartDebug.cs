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

        internal IReadOnlyList<IFieldLayout> FieldLayouts => _fieldLayouts;

        public static SmartDebug Instance
        {
            get
            {
                Initialize();
                return _instance;
            }
        }

        public bool IsShownMenu => _canvas.gameObject.activeSelf;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnSceneLoaded()
        {
            if (!SDSettings.Instance.IsAutoInitialize) return;
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

        public void OpenMenu(bool showLog = false)
        {
            if (showLog)
            {
                _canvas.Open<LogTabContent>();
            }
            else
            {
                _canvas.Open();
            }
        }

        public void CloseMenu()
        {
            _canvas.Close();
        }

        internal void HideMenu()
        {
            _canvas.SetAlpha(0f);
        }

        private void LateUpdate()
        {
            if (!IsShownMenu)
            {
                foreach (var detector in _openEventDetectors)
                {
                    if (!detector.IsTriggered()) continue;
                    OpenMenu(ErrorIndicator.HasError);
                    break;
                }
            }
            else
            {
                foreach (var detector in _closeEventDetectors)
                {
                    if (!detector.IsTriggered()) continue;
                    CloseMenu();
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