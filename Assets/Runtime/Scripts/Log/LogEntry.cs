namespace SmartDebugger
{
    public readonly struct LogEntry
    {
        public readonly int Id;
        public readonly string Message;
        public readonly string[] StackTrace;
        public readonly LogTypes Types;
        public readonly float Time;

        public LogEntry(int id, string message, string stackTrace, UnityEngine.LogType type, float time)
        {
            Id = id;
            Message = message;
            StackTrace = stackTrace.Split('\n');
            Time = time;
            Types = type switch
            {
                UnityEngine.LogType.Log => LogTypes.Info,
                UnityEngine.LogType.Warning => LogTypes.Warning,
                _ => LogTypes.Error
            };
        }
    }
}