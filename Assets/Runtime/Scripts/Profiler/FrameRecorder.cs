using System;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace SmartDebugger
{
    internal class FrameRecorder : IDisposable
    {
        internal int SampleCount { get; }
        internal Vector4[] CpuSamples { get; }
        internal Vector4[] MemorySamples { get; }
        internal FloatQueue DeltaTimes { get; }
        internal int SampleOffset { get; private set; }
        internal long UsedMonoBytes { get; private set; }
        internal long TotalMonoBytes { get; private set; }
        internal long UsedBytes { get; private set; }
        internal long TotalBytes { get; private set; }

        private ProfilerRecorder _cpuTotalFrameTime;
        private ProfilerRecorder _cpuMainThreadFrameTime;
        private ProfilerRecorder _cpuRenderThreadFrameTime;
        private ProfilerRecorder _physics2DSimulate;
        private ProfilerRecorder _physicsProcessing;

        private int _sampleOffset;

        internal FrameRecorder(int sampleCount = 180)
        {
            SampleCount = sampleCount;
            CpuSamples = new Vector4[sampleCount];
            MemorySamples = new Vector4[sampleCount];
            DeltaTimes = new FloatQueue(sampleCount);

            _cpuTotalFrameTime = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Total Frame Time");
            _cpuMainThreadFrameTime = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Main Thread Frame Time");
            _cpuRenderThreadFrameTime = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Render Thread Frame Time");
            _physicsProcessing = ProfilerRecorder.StartNew(ProfilerCategory.Physics, "FixedUpdate.PhysicsFixedUpdate");
            _physics2DSimulate = ProfilerRecorder.StartNew(ProfilerCategory.Physics, "Physics2D.Simulate");
        }

        internal void Sample()
        {
            SampleOffset = _sampleOffset;
            SampleCps();
            SampleMemory();
            _sampleOffset = (_sampleOffset + 1) % SampleCount;
        }

        private void SampleCps()
        {
            var cpuTotalFrameTime = GetMs(_cpuTotalFrameTime);
            var cpuMainThreadFrameTime = GetMs(_cpuMainThreadFrameTime);
            var cpuRenderThreadFrameTime = GetMs(_cpuRenderThreadFrameTime);
            var physicsTime = GetMs(_physicsProcessing);
            physicsTime +=  GetMs(_physics2DSimulate);
            var waitMs = Mathf.Max(0, cpuTotalFrameTime - cpuMainThreadFrameTime - cpuRenderThreadFrameTime);
            CpuSamples[SampleOffset] = new Vector4(cpuMainThreadFrameTime - physicsTime, cpuRenderThreadFrameTime, physicsTime, waitMs);
            DeltaTimes.Enqueue(Time.unscaledDeltaTime);
        }

        private void SampleMemory()
        {
            UsedMonoBytes = Profiler.GetMonoUsedSizeLong();
            TotalMonoBytes = Profiler.GetMonoHeapSizeLong();
            UsedBytes = Profiler.GetTotalAllocatedMemoryLong();
            TotalBytes = Profiler.GetTotalReservedMemoryLong();
            var monoNorm = Mathf.Clamp01((float)(UsedMonoBytes / (double)TotalMonoBytes));
            var totalNorm = Mathf.Clamp01((float)(UsedBytes / (double)TotalBytes));
            MemorySamples[SampleOffset] = new Vector4(monoNorm, totalNorm);
        }

        private static float GetMs(ProfilerRecorder recorder)
        {
            return recorder is { Valid: true, Count: > 0 } ? (float)recorder.LastValueAsDouble / 1000000f : 0f;
        }

        public void Dispose()
        {
            _cpuTotalFrameTime.Dispose();
            _cpuMainThreadFrameTime.Dispose();
            _cpuRenderThreadFrameTime.Dispose();
            _physics2DSimulate.Dispose();
            _physicsProcessing.Dispose();
        }
    }
}