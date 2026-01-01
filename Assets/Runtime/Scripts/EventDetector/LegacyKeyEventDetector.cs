using UnityEngine;

namespace SmartDebugger
{
    public sealed class LegacyKeyEventDetector : IEventDetector
    {
        private readonly KeyShortcut _shortcut;

        public LegacyKeyEventDetector(KeyShortcut shortcut)
        {
            _shortcut = shortcut;
        }

        public bool IsTriggered()
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