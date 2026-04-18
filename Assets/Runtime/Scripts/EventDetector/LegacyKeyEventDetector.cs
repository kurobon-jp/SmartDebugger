using UnityEngine;

namespace SmartDebugger
{
    internal sealed class LegacyKeyEventDetector : IEventDetector
    {
        private readonly KeyShortcut _shortcut;

        internal LegacyKeyEventDetector(KeyShortcut shortcut)
        {
            _shortcut = shortcut;
        }

        bool IEventDetector.IsTriggered()
        {
            if (!Input.GetKeyDown(_shortcut.KeyCode))
                return false;

            if ((_shortcut.Modifiers & ModifierKeys.Shift) != 0 &&
                !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return false;

            if ((_shortcut.Modifiers & ModifierKeys.Ctrl) != 0 &&
                !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                return false;

            if ((_shortcut.Modifiers & ModifierKeys.Alt) != 0 &&
                !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
                return false;

            return true;
        }
    }

}