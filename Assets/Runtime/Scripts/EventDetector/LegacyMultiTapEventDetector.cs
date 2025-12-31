using UnityEngine;

namespace SmartDebugger
{
    public sealed class LegacyMultiTapEventDetector : IEventDetector
    {
        private readonly TapEvent _tapEvent;
        private int _currentTapCount = 0;
        private float _lastTapTime = 0f;

        public LegacyMultiTapEventDetector(TapEvent tapEvent)
        {
            _tapEvent = tapEvent;
        }

        public bool IsTriggered()
        {
            if (Input.touchCount != _tapEvent.TouchCount)
                return false;

            var allEnded = false;
            for (var i = 0; i < _tapEvent.TouchCount; i++)
            {
                allEnded |= Input.GetTouch(i).phase == TouchPhase.Ended;
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