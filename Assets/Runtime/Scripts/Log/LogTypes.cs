using System;

namespace SmartDebugger
{
    [Flags]
    public enum LogTypes
    {
        None = 0,
        Info = 1 << 0,
        Warning = 1 << 1,
        Error = 1 << 2,
        All = Info | Warning | Error
    }

    public static class LogTypeExtensions
    {
        public static int GetIndex(this LogTypes types)
        {
            switch (types)
            {
                case LogTypes.Warning:
                    return 1;
                case LogTypes.Error:
                    return 2;
            }

            return 0;
        }
    }
}