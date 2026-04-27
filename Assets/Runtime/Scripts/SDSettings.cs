using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SmartDebugger
{
    [CreateAssetMenu(menuName = "SmartDebugger/SD Settings", fileName = "SDSettings")]
    public class SDSettings : ScriptableObject
    {
        private static SDSettings _instance;

        public static SDSettings Instance
        {
            get
            {
#if UNITY_EDITOR
                if (_instance == null)
                {
                    _instance = UnityEditor.PlayerSettings.GetPreloadedAssets().OfType<SDSettings>().FirstOrDefault();
                }
#endif
                return _instance == null
                    ? throw new Exception(
                        "SDSettings is not preloaded. Please make sure it is included in Preloaded Assets.")
                    : _instance;
            }
        }

        [SerializeField] private bool _autoInitialize = true;
        [SerializeField] private bool _dontDestroyOnLoad = true;
        [SerializeField] private bool _pauseOnDebugMenu;
        [SerializeField] private bool _autoGenerateEventSystem = true;
        [SerializeField] private bool _showErrorIndicator = true;

        [SerializeField] private KeyShortcut _openShortcut = new()
        {
            KeyCode = KeyCode.F1,
            Modifiers = ModifierKeys.Ctrl | ModifierKeys.Shift
        };

        [SerializeField] private KeyShortcut _closeShortcut = new()
        {
            KeyCode = KeyCode.Escape
        };

        [SerializeField] private TapEvent _openTapEvent = new()
        {
            TouchCount = 3,
            TapCount = 3,
            Interval = 0.5f
        };

        [SerializeField] private MainTabContent[] _mainTabContents;
        [SerializeField] private GameObject[] _prefabs;
        [SerializeField] private int _canvasSortingOrder = 100;
        [SerializeField] private BugReporter _bugReporter;

        public bool IsAutoInitialize => _autoInitialize;
        public bool IsDontDestroyOnLoad => _dontDestroyOnLoad;
        public bool IsPauseOnDebugMenu => _pauseOnDebugMenu;
        public bool IsAutoGenerateEventSystem => _autoGenerateEventSystem;
        public bool IsShowErrorIndicator => _showErrorIndicator;
        public KeyShortcut OpenShortcut => _openShortcut;
        public KeyShortcut CloseShortcut => _closeShortcut;
        public TapEvent OpenTapEvent => _openTapEvent;
        public MainTabContent[] MainTabContents => _mainTabContents;
        public int CanvasSortingOrder => _canvasSortingOrder;
        public BugReporter BugReporter => _bugReporter;

        public T LoadPrefab<T>(string prefabName) where T : UnityEngine.Object
        {
            foreach (var prefab in _prefabs)
            {
                if (prefab.name != prefabName) continue;
                if (prefab is T t || prefab.TryGetComponent(out t)) return t;
            }

            throw new FileNotFoundException($"Prefab of type {typeof(T)} not found in SDSettings.");
        }

#if !UNITY_EDITOR
        private void OnEnable()
        {
            _instance = this;
        }
#endif
    }
}