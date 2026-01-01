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