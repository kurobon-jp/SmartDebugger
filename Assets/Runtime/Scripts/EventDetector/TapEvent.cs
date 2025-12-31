using System;
using UnityEngine;

namespace SmartDebugger
{
    [Serializable]
    public struct TapEvent
    {
        [Range(0, 5)]
        public int TouchCount;
        [Range(0, 5)]
        public int TapCount;
        [Range(0.1f, 1f)]
        public float Interval;
        
        public bool IsEnable => TouchCount > 0 && TapCount > 0;
    }
}