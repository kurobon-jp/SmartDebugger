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
        internal float CpuTotalFrameTime { get; private set; }
        internal float CpuMainThreadFrameTime { get; private set; }
        internal float CpuRenderThreadFrameTime { get; private set; }
        internal float GpuFrameTime { get; private set; }

        private ProfilerRecorder _cpuTotalFrameTime;
        private ProfilerRecorder _cpuMainThreadFrameTime;
        private ProfilerRecorder _cpuRenderThreadFrameTime;
        private ProfilerRecorder _gpuFrameTime;
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
            _cpuMainThreadFrameTime =
                ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Main Thread Frame Time");
            _cpuRenderThreadFrameTime =
                ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Render Thread Frame Time");
            _gpuFrameTime = ProfilerRecorder.StartNew(ProfilerCategory.Render, "GPU Frame Time");
            _physicsProcessing = ProfilerRecorder.StartNew(ProfilerCategory.Physics, "FixedUpdate.PhysicsFixedUpdate");
            _physics2DSimulate = ProfilerRecorder.StartNew(ProfilerCategory.Physics, "Physics2D.Simulate");
        }

        internal void Sample()
        {
            SampleOffset = _sampleOffset;
            SampleCpu();
            SampleMemory();
            _sampleOffset = (_sampleOffset + 1) % SampleCount;
        }

        private void SampleCpu()
        {
            CpuTotalFrameTime = GetMs(_cpuTotalFrameTime);
            CpuMainThreadFrameTime = GetMs(_cpuMainThreadFrameTime);
            CpuRenderThreadFrameTime = GetMs(_cpuRenderThreadFrameTime);
            GpuFrameTime = GetMs(_gpuFrameTime);
            var physicsTime = GetMs(_physicsProcessing);
            physicsTime += GetMs(_physics2DSimulate);
            // CPU Main Thread Frame Time already includes physics. Keep the
            // categories non-negative and avoid subtracting render-thread time
            // from main-thread time: they are measured on different threads.
            var scriptTime = Mathf.Max(0, CpuMainThreadFrameTime - physicsTime);
            var otherTime = Mathf.Max(0,
                CpuTotalFrameTime - scriptTime - physicsTime - CpuRenderThreadFrameTime);
            CpuSamples[SampleOffset] = new Vector4(scriptTime,
                CpuRenderThreadFrameTime,
                physicsTime, otherTime);
            DeltaTimes.Enqueue(CpuTotalFrameTime / 1000f);
        }

        private void SampleMemory()
        {
            UsedMonoBytes = Profiler.GetMonoUsedSizeLong();
            TotalMonoBytes = Profiler.GetMonoHeapSizeLong();
            UsedBytes = Profiler.GetTotalAllocatedMemoryLong();
            TotalBytes = Profiler.GetTotalReservedMemoryLong();
            var monoNorm = TotalMonoBytes > 0
                ? Mathf.Clamp01((float)(UsedMonoBytes / (double)TotalMonoBytes))
                : 0f;
            var totalNorm = TotalBytes > 0
                ? Mathf.Clamp01((float)(UsedBytes / (double)TotalBytes))
                : 0f;
            MemorySamples[SampleOffset] = new Vector4(monoNorm, totalNorm);
        }

        private static float GetMs(ProfilerRecorder recorder)
        {
            return recorder is { Valid: true, Count: > 0 } ? recorder.LastValue / 1000000f : 0f;
        }

        public void Dispose()
        {
            _cpuTotalFrameTime.Dispose();
            _cpuMainThreadFrameTime.Dispose();
            _cpuRenderThreadFrameTime.Dispose();
            _gpuFrameTime.Dispose();
            _physics2DSimulate.Dispose();
            _physicsProcessing.Dispose();
        }
    }
}
