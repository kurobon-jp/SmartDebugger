namespace SmartDebugger
{
    public static class EventDetectorFactory
    {
        public static IEventDetector CreateKeyEventDetector(KeyShortcut shortcut)
        {
#if ENABLE_INPUT_SYSTEM
            return new InputSystemKeyEventDetector(shortcut);
#else
            return new LegacyKeyEventDetector(shortcut);
#endif
        }

        public static IEventDetector CreateMultiTapEventDetector(TapEvent tapEvent)
        {
#if ENABLE_INPUT_SYSTEM
            return new InputSystemMultiTapEventDetector(tapEvent);
#else
            return new LegacyMultiTapEventDetector(tapEvent);
#endif
        }
    }
}