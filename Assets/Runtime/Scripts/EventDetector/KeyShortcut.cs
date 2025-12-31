using System;
using UnityEngine;

namespace SmartDebugger
{
    [Serializable]
    public struct KeyShortcut
    {
        public KeyCode Key;
        public ModifierKeys Modifiers;

        public bool IsEnable => Key != KeyCode.None;
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