using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace SmartDebugger
{
    internal static class InputSystemIntegration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register()
        {
            EventDetectorFactory.KeyDetectorFactory = shortcut => new InputSystemKeyEventDetector(shortcut);
            EventDetectorFactory.TapDetectorFactory = tap => new InputSystemMultiTapEventDetector(tap);
            EventDetectorFactory.AddInputModule = go => go.AddComponent<InputSystemUIInputModule>();
        }
    }
}
