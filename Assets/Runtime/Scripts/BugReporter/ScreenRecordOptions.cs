#if ENABLE_INSTANTREPLAY && !EXCLUDE_INSTANTREPLAY
using System;
using UnityEngine;

namespace SmartDebugger
{
    [Serializable]
    public class ScreenRecordOptions
    {
        [SerializeField] private uint _width = 1280;
        [SerializeField] private uint _height = 720;
        [SerializeField] private uint _fps = 30;
        [SerializeField] private uint _videoBitrate = 2500000; // 2.5 Mbps
        [SerializeField] private int _maxNumberOfRawFrameBuffers = 30;
        [SerializeField] private uint _fixedFrameRate;
        [SerializeField] private int _videoInputQueueSize = 5;

        [SerializeField] private uint _sampleRate = 44100;
        [SerializeField] private uint _channels = 2;
        [SerializeField] private uint _audioBitrate = 128000; // 128 kbps
        [SerializeField] private double _audioInputQueueSizeSeconds = 1.0;

        [SerializeField] private uint _maxMemoryUsageBytesForCompressedFrames = 20 * 1024 * 1024; // 20 MiB

        public uint Width => _width;

        public uint Height => _height;

        public uint Fps => _fps;

        public uint VideoBitrate => _videoBitrate;

        public int MaxNumberOfRawFrameBuffers => _maxNumberOfRawFrameBuffers;

        public uint FixedFrameRate => _fixedFrameRate;

        public int VideoInputQueueSize => _videoInputQueueSize;

        public uint SampleRate => _sampleRate;

        public uint Channels => _channels;

        public uint AudioBitrate => _audioBitrate;

        public double AudioInputQueueSizeSeconds => _audioInputQueueSizeSeconds;

        public uint MaxMemoryUsageBytesForCompressedFrames => _maxMemoryUsageBytesForCompressedFrames;
    }
}
#endif