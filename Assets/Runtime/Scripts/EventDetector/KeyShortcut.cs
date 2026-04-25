using System;
using UnityEngine;

namespace SmartDebugger
{
    [Serializable]
    public struct KeyShortcut
    {
        public KeyCode KeyCode;
        public ModifierKeys Modifiers;

        public bool IsEnable => KeyCode != KeyCode.None;
#if UNITY_EDITOR
        public override string ToString()
        {
            return Modifiers == ModifierKeys.None
                ? KeyCode.ToString()
                : $"{Modifiers.ToString().Replace(", ", "+")}+{KeyCode}";
        }
#endif
    }

    [Flags]
    public enum ModifierKeys
    {
        None = 0,
        Shift = 1 << 0,
        Ctrl = 1 << 1,
        Alt = 1 << 2,
    }
}