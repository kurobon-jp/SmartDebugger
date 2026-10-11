using System;
using System.Threading.Tasks;

namespace SmartDebugger
{
    internal interface IScreenRecorder : IDisposable
    {
        Task<string> StopAndExportAsync();
    }

    internal static class ScreenRecording
    {
        internal static Func<ScreenRecordOptions, IScreenRecorder> Create;
    }
}
