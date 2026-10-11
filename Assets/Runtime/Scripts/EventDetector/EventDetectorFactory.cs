using System;
using UnityEngine;

namespace SmartDebugger
{
    internal static class EventDetectorFactory
    {
        internal static Func<KeyShortcut, IEventDetector> KeyDetectorFactory;
        internal static Func<TapEvent, IEventDetector> TapDetectorFactory;
        internal static Action<GameObject> AddInputModule;

        public static IEventDetector CreateKeyEventDetector(KeyShortcut shortcut)
        {
            return KeyDetectorFactory != null
                ? KeyDetectorFactory(shortcut)
                : new LegacyKeyEventDetector(shortcut);
        }

        public static IEventDetector CreateMultiTapEventDetector(TapEvent tapEvent)
        {
            return TapDetectorFactory != null
                ? TapDetectorFactory(tapEvent)
                : new LegacyMultiTapEventDetector(tapEvent);
        }
    }
}
