#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace SmartDebugger
{
    public sealed class InputSystemMultiTapEventDetector : IEventDetector
    {
        private readonly TapEvent _tapEvent;

        private int _currentTapCount = 0;
        private float _lastTapTime = 0f;

        public InputSystemMultiTapEventDetector(TapEvent tapEvent)
        {
            _tapEvent = tapEvent;
            EnhancedTouchSupport.Enable();
        }

        public bool IsTriggered()
        {
            var touches = Touch.activeTouches;
            if (touches.Count != 0)
            {
                
            }
            if (touches.Count != _tapEvent.TouchCount)
                return false;

            var allEnded = false;
            foreach (var t in touches)
            {
                allEnded |= t.phase is UnityEngine.InputSystem.TouchPhase.Ended or UnityEngine.InputSystem.TouchPhase.Canceled;
            }

            if (!allEnded)
                return false;

            var now = Time.time;

            if (now - _lastTapTime > _tapEvent.Interval)
                _currentTapCount = 0;

            _currentTapCount++;
            _lastTapTime = now;
            if (_currentTapCount >= _tapEvent.TapCount)
            {
                _currentTapCount = 0;
                return true;
            }

            return false;
        }
    }
}
#endif