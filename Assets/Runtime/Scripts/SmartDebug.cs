using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SmartDebugger
{
    public class SmartDebug : MonoBehaviour
    {
        private static SmartDebug _instance;

        [SerializeField] private SDCanvas _canvas;
        private float _timeScale;
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

        public bool IsShownCanvas => _canvas.gameObject.activeSelf;

        [RuntimeInitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            if (SDSettings.Instance.IsAutoInitialize)
            {
                Initialize();
            }
        }

        public static void Initialize()
        {
            if (_instance != null) return;
            var prefab = Resources.Load<GameObject>("Prefabs/SmartDebug");
            var go = Instantiate(prefab);
            if (SDSettings.Instance.IsDontDestroyOnLoad)
            {
                DontDestroyOnLoad(go);
            }

            _instance = go.GetComponent<SmartDebug>();
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

        private void OnEnable()
        {
            if (!SDSettings.Instance.IsPauseOnDebugMenu) return;
            _timeScale = Time.timeScale;
            Time.timeScale = 0;
        }

        private void OnDisable()
        {
            if (!SDSettings.Instance.IsPauseOnDebugMenu) return;
            Time.timeScale = _timeScale;
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        public void OpenMenu()
        {
            _canvas.gameObject.SetActive(true);
        }

        public void CloseMenu()
        {
            _canvas.gameObject.SetActive(false);
        }

        public void ToggleMenu()
        {
            _canvas.gameObject.SetActive(!IsShownCanvas);
        }

        private void LateUpdate()
        {
            foreach (var detector in _openEventDetectors)
            {
                if (detector.IsTriggered())
                {
                    ToggleMenu();
                    break;
                }
            }

            foreach (var detector in _closeEventDetectors)
            {
                if (detector.IsTriggered())
                {
                    CloseMenu();
                    break;
                }
            }
        }

        public void AddFieldLayout(IFieldLayout category)
        {
            _fieldLayouts.Add(category);
        }

        public void RemoveFieldLayout(IFieldLayout category)
        {
            _fieldLayouts.Remove(category);
        }
    }
}