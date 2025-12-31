#if ENABLE_INPUT_SYSTEM
using System.Collections.Generic;
using UnityEngine;

namespace SmartDebugger
{
    using UnityEngine.InputSystem;

    public sealed class InputSystemKeyEventDetector : IEventDetector
    {
        private readonly Key _key;
        private readonly ModifierKeys _modifiers;

        private static readonly Dictionary<KeyCode, Key> Keymap = new()
        {
            { KeyCode.A, Key.A },
            { KeyCode.B, Key.B },
            { KeyCode.C, Key.C },
            { KeyCode.D, Key.D },
            { KeyCode.E, Key.E },
            { KeyCode.F, Key.F },
            { KeyCode.G, Key.G },
            { KeyCode.H, Key.H },
            { KeyCode.I, Key.I },
            { KeyCode.J, Key.J },
            { KeyCode.K, Key.K },
            { KeyCode.L, Key.L },
            { KeyCode.M, Key.M },
            { KeyCode.N, Key.N },
            { KeyCode.O, Key.O },
            { KeyCode.P, Key.P },
            { KeyCode.Q, Key.Q },
            { KeyCode.R, Key.R },
            { KeyCode.S, Key.S },
            { KeyCode.T, Key.T },
            { KeyCode.U, Key.U },
            { KeyCode.V, Key.V },
            { KeyCode.W, Key.W },
            { KeyCode.X, Key.X },
            { KeyCode.Y, Key.Y },
            { KeyCode.Z, Key.Z },

            { KeyCode.F1, Key.F1 },
            { KeyCode.F2, Key.F2 },
            { KeyCode.F3, Key.F3 },
            { KeyCode.F4, Key.F4 },
            { KeyCode.F5, Key.F5 },
            { KeyCode.F6, Key.F6 },
            { KeyCode.F7, Key.F7 },
            { KeyCode.F8, Key.F8 },
            { KeyCode.F9, Key.F9 },
            { KeyCode.F10, Key.F10 },
            { KeyCode.F11, Key.F11 },
            { KeyCode.F12, Key.F12 },

            { KeyCode.Alpha0, Key.Digit0 },
            { KeyCode.Alpha1, Key.Digit1 },
            { KeyCode.Alpha2, Key.Digit2 },
            { KeyCode.Alpha3, Key.Digit3 },
            { KeyCode.Alpha4, Key.Digit4 },
            { KeyCode.Alpha5, Key.Digit5 },
            { KeyCode.Alpha6, Key.Digit6 },
            { KeyCode.Alpha7, Key.Digit7 },
            { KeyCode.Alpha8, Key.Digit8 },
            { KeyCode.Alpha9, Key.Digit9 },

            { KeyCode.Space, Key.Space },
            { KeyCode.Return, Key.Enter },
            { KeyCode.Escape, Key.Escape },
            { KeyCode.Tab, Key.Tab },
            { KeyCode.Backspace, Key.Backspace },
            { KeyCode.Delete, Key.Delete },
            { KeyCode.LeftArrow, Key.LeftArrow },
            { KeyCode.RightArrow, Key.RightArrow },
            { KeyCode.UpArrow, Key.UpArrow },
            { KeyCode.DownArrow, Key.DownArrow },
        };

        public InputSystemKeyEventDetector(KeyShortcut shortcut)
        {
            _key = Keymap[shortcut.Key];
            _modifiers = shortcut.Modifiers;
        }

        public bool IsTriggered()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            if (!keyboard[_key].wasPressedThisFrame)
                return false;

            if ((_modifiers & ModifierKeys.Shift) != 0 &&
                !keyboard.shiftKey.isPressed)
                return false;

            if ((_modifiers & ModifierKeys.Ctrl) != 0 &&
                !keyboard.ctrlKey.isPressed)
                return false;

            if ((_modifiers & ModifierKeys.Alt) != 0 &&
                !keyboard.altKey.isPressed)
                return false;

            return true;
        }
    }
}
#endif