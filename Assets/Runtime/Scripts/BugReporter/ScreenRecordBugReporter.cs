using System.Threading.Tasks;
using UnityEngine;

namespace SmartDebugger
{
    public abstract class ScreenRecordBugReporter : BugReporter
    {
        private IScreenRecorder _screenRecorder;

        public bool IsScreenRecordingAvailable => ScreenRecording.Create != null;

        [SerializeField] private ScreenRecordOptions _screenRecordOptions = new();

        internal override void Initialize()
        {
            IncludeScreenRecord.OnValueChanged -= OnIncludeScreenRecordChanged;
            IncludeScreenRecord.OnValueChanged += OnIncludeScreenRecordChanged;
            if (IncludeScreenRecord)
            {
                StartRecording();
            }
        }

        private void OnDisable()
        {
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
            StopRecording();
            _screenRecorder = ScreenRecording.Create?.Invoke(_screenRecordOptions);
        }

        private void StopRecording()
        {
            _screenRecorder?.Dispose();
            _screenRecorder = null;
        }

        public async Task<string> StopRecordingAndExportAsync()
        {
            if (_screenRecorder == null) return null;
            try
            {
                return await _screenRecorder.StopAndExportAsync();
            }
            finally
            {
                if (IncludeScreenRecord)
                {
                    StartRecording();
                }
            }
        }
    }
}
