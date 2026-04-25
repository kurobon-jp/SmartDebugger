#if ENABLE_INSTANTREPLAY && !EXCLUDE_INSTANTREPLAY
using System;
using System.Threading.Tasks;
using InstantReplay;
using UnityEngine;

namespace SmartDebugger
{
    public class ScreenRecorder : IDisposable
    {
        private readonly RealtimeInstantReplaySession _session;
        private bool _disposed;

        public ScreenRecorder(ScreenRecordOptions options)
        {
            var encodingOptions = new RealtimeEncodingOptions
            {
                VideoOptions = new UniEnc.VideoEncoderOptions
                {
                    Width = options.Width,
                    Height = options.Height,
                    FpsHint = options.Fps,
                    Bitrate = options.VideoBitrate
                },
                AudioOptions = new UniEnc.AudioEncoderOptions
                {
                    SampleRate = options.SampleRate,
                    Channels = options.Channels,
                    Bitrate = options.AudioBitrate
                },
                MaxNumberOfRawFrameBuffers = options.MaxNumberOfRawFrameBuffers,
                MaxMemoryUsageBytesForCompressedFrames = options.MaxMemoryUsageBytesForCompressedFrames,
                FixedFrameRate = options.FixedFrameRate > 0 ? options.FixedFrameRate : null,
                VideoInputQueueSize = options.VideoInputQueueSize,
                AudioInputQueueSizeSeconds = options.AudioInputQueueSizeSeconds
            };

            var go = new GameObject(nameof(UnityAudioSampleProvider));
            var provider = go.AddComponent<UnityAudioSampleProvider>();
            _session = new RealtimeInstantReplaySession(encodingOptions, audioSampleProvider: provider);
            Application.quitting += Dispose;
            Application.focusChanged += OnFocusChanged;
        }

        private void OnFocusChanged(bool hasFocus)
        {
            if (hasFocus)
            {
                _session.Resume();
            }
            else
            {
                _session.Pause();
            }
        }

        public async Task<string> StopAndExportAsync()
        {
            if (_disposed) return null;
            _disposed = true;
            Application.quitting -= Dispose;
            Application.focusChanged -= OnFocusChanged;
            try
            {
                return await _session.StopAndExportAsync();
            }
            finally
            {
                _session.Dispose();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Application.quitting -= Dispose;
            Application.focusChanged -= OnFocusChanged;
            _session.Dispose();
        }
    }

    [DisallowMultipleComponent]
    internal class UnityAudioSampleProvider : MonoBehaviour, IAudioSampleProvider
    {
        public event IAudioSampleProvider.ProvideAudioSamples OnProvideAudioSamples;

        private AudioListener _audioListener;
        private UnityAudioSampleReceiver _receiver;
        private float _elapsed;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            FindAndSetupListener();
        }

        private void FindAndSetupListener()
        {
            var audioListener = FindAnyObjectByType<AudioListener>();
            if (audioListener == null) return;
            _audioListener = audioListener;
            _receiver = audioListener.gameObject.AddComponent<UnityAudioSampleReceiver>();
            _receiver.OnProvideAudioSamples += ProvideAudioSamples;
        }

        private void Update()
        {
            if (_audioListener != null) return;
            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed < 1f) return;
            _elapsed = 0f;
            FindAndSetupListener();
        }

        private void ProvideAudioSamples(ReadOnlySpan<float> samples, int channels, int sampleRate,
            double timestamp)
        {
            OnProvideAudioSamples?.Invoke(samples, channels, sampleRate, timestamp);
        }

        public void Dispose()
        {
            if (_receiver != null)
            {
                _receiver.OnProvideAudioSamples -= ProvideAudioSamples;
                Destroy(_receiver);
            }

            Destroy(gameObject);
        }
    }

    internal class UnityAudioSampleReceiver : MonoBehaviour
    {
        private int _sampleRate;

        #region Event Functions

        private void Update()
        {
            _sampleRate = AudioSettings.outputSampleRate;
        }

        private void OnEnable()
        {
            _sampleRate = AudioSettings.outputSampleRate;
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            OnProvideAudioSamples?.Invoke(new ReadOnlySpan<float>(data), channels, _sampleRate, AudioSettings.dspTime);
        }

        #endregion

        public event IAudioSampleProvider.ProvideAudioSamples OnProvideAudioSamples;
    }
}
#endif