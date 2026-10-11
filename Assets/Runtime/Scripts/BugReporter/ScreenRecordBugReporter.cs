using System.Threading.Tasks;
using UnityEngine;

namespace SmartDebugger
{
    public abstract class ScreenRecordBugReporter : BugReporter
    {
        private IScreenRecorder _screenRecorder;
        private Task<string> _exportTask;
        private string _retainedExport;
        private bool _initialized;

        internal Task PendingExport => _exportTask is { IsCompleted: false } ? _exportTask : null;

        public bool IsScreenRecordingAvailable => ScreenRecording.Create != null;

        [SerializeField] private ScreenRecordOptions _screenRecordOptions = new();

        internal override void Initialize()
        {
            _initialized = true;
            IncludeScreenRecord.OnValueChanged -= OnIncludeScreenRecordChanged;
            IncludeScreenRecord.OnValueChanged += OnIncludeScreenRecordChanged;
            if (IncludeScreenRecord)
            {
                StartRecording();
            }
        }

        private void OnDisable()
        {
            _initialized = false;
            IncludeScreenRecord.OnValueChanged -= OnIncludeScreenRecordChanged;
            StopRecording();
        }

        private void OnIncludeScreenRecordChanged(SerializeVariable<bool> variable)
        {
            if (variable)
            {
                StartRecording();
            }
            else
            {
                StopRecording();
            }
        }

        private void StartRecording()
        {
            if (PendingExport != null) return;
            StopRecording();
            _screenRecorder = ScreenRecording.Create?.Invoke(_screenRecordOptions);
        }

        private void StopRecording()
        {
            _screenRecorder?.Dispose();
            _screenRecorder = null;
        }

        /// <summary>Exports the current recording. Concurrent callers share the active export.</summary>
        /// <remarks>Ending a caller's wait does not cancel the export operation.</remarks>
        public Task<string> StopRecordingAndExportAsync()
        {
            if (PendingExport != null) return _exportTask;
            if (_screenRecorder == null) return Task.FromResult<string>(null);
            var recorder = _screenRecorder;
            _screenRecorder = null;
            return _exportTask = ExportAsync(recorder);
        }

        internal Task<string> ExportForReportAsync() => _retainedExport != null
            ? Task.FromResult(_retainedExport) : StopRecordingAndExportAsync();

        internal void ClaimExport(string path)
        {
            if (_retainedExport == path) _retainedExport = null;
        }

        private async Task<string> ExportAsync(IScreenRecorder recorder)
        {
            try
            {
                return _retainedExport = await recorder.StopAndExportAsync();
            }
            finally
            {
                recorder.Dispose();
                // This operation owns its detached recorder until export terminates.
                // Do not restart after ScriptableObject shutdown.
                if (_initialized && IncludeScreenRecord)
                {
                    _screenRecorder = ScreenRecording.Create?.Invoke(_screenRecordOptions);
                }
            }
        }
    }
}
