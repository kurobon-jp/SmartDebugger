using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SmartDebugger
{
    public abstract class ScreenRecordBugReporter : BugReporter
    {
#if ENABLE_INSTANTREPLAY && !EXCLUDE_INSTANTREPLAY
        private ScreenRecorder _screenRecorder;

        [SerializeField] private ScreenRecordOptions _screenRecordOptions = new();

        internal override void Initialize()
        {
            IncludeScreenRecord.OnValueChanged += OnIncludeScreenRecordChanged;
            if (!IncludeScreenRecord) return;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            IncludeScreenRecord.OnValueChanged -= OnIncludeScreenRecordChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            StopRecording();
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            StartRecording();
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
            _screenRecorder = new ScreenRecorder(_screenRecordOptions);
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
#endif
    }
}
