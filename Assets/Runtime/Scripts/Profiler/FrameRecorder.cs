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
        private ProfilerRecorder _scriptUpdate;
        private ProfilerRecorder _scriptFixedUpdate;
        private ProfilerRecorder _scriptLateUpdate;
        private ProfilerRecorder _scriptDelayedTasks;
        private ProfilerRecorder _cameraRender;
        private ProfilerRecorder _renderPipeline;

        private int _sampleOffset;
#if UNITY_EDITOR
        private int _diagnosticFrames;
#endif

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
            _physicsProcessing = RecordMainThread("FixedUpdate.PhysicsFixedUpdate");
            _physics2DSimulate = RecordMainThread("FixedUpdate.Physics2DFixedUpdate");
            _scriptUpdate = RecordMainThread("Update.ScriptRunBehaviourUpdate");
            _scriptFixedUpdate = RecordMainThread("FixedUpdate.ScriptRunBehaviourFixedUpdate");
            _scriptLateUpdate = RecordMainThread("PreLateUpdate.ScriptRunBehaviourLateUpdate");
            _scriptDelayedTasks = RecordMainThread("Update.ScriptRunDelayedTasks");
            _cameraRender = RecordMainThread("Camera.Render");
            _renderPipeline = RecordMainThread("RenderPipelineManager.DoRenderLoop_Internal");
        }

        private static ProfilerRecorder RecordMainThread(string marker)
        {
            // Match by name regardless of the category assigned by the Editor version.
            // Sum repeated FixedUpdates/cameras, but exclude render/worker threads.
            return new ProfilerRecorder(marker, 1, ProfilerRecorderOptions.Default
                | ProfilerRecorderOptions.StartImmediately
                | ProfilerRecorderOptions.CollectOnlyOnCurrentThread);
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
            var scriptTime = GetMs(_scriptUpdate) + GetMs(_scriptFixedUpdate)
                + GetMs(_scriptLateUpdate) + GetMs(_scriptDelayedTasks);
            // The SRP loop can contain Camera.Render: select the outer loop rather
            // than adding both. Thread frame counters are not category timings.
            var renderingTime = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null
                ? GetMs(_renderPipeline) : GetMs(_cameraRender);
            var totalTime = CpuTotalFrameTime > 0f
                ? CpuTotalFrameTime : Time.unscaledDeltaTime * 1000f;
            CpuSamples[SampleOffset] = ComposeCpuSamples(totalTime, scriptTime, renderingTime, physicsTime);
            DeltaTimes.Enqueue(totalTime / 1000f);
#if UNITY_EDITOR
            // Temporary diagnosis for the reported all-Other result in 2022.1.
            if (++_diagnosticFrames == 120)
            {
                Debug.Log($"[SmartDebugger CPU diagnostics] Unity {Application.unityVersion}; "
                    + $"Scripts={scriptTime:F3}ms Rendering={renderingTime:F3}ms Physics={physicsTime:F3}ms "
                    + $"Total={totalTime:F3}ms Main={CpuMainThreadFrameTime:F3}ms RenderThread={CpuRenderThreadFrameTime:F3}ms; "
                    + $"Update: valid={_scriptUpdate.Valid}, count={_scriptUpdate.Count}, raw={_scriptUpdate.LastValue}; "
                    + $"Physics: valid={_physicsProcessing.Valid}, count={_physicsProcessing.Count}, raw={_physicsProcessing.LastValue}; "
                    + $"Camera: valid={_cameraRender.Valid}, count={_cameraRender.Count}, raw={_cameraRender.LastValue}; "
                    + $"SRP: valid={_renderPipeline.Valid}, count={_renderPipeline.Count}, raw={_renderPipeline.LastValue}");
            }
#endif
        }

        internal static Vector4 ComposeCpuSamples(float totalTime, float scripts, float rendering, float physics)
        {
            scripts = Mathf.Max(0f, scripts);
            rendering = Mathf.Max(0f, rendering);
            physics = Mathf.Max(0f, physics);
            // Other includes unclassified main-thread work, idle time and waits.
            return new Vector4(scripts, rendering, physics,
                Mathf.Max(0f, totalTime - scripts - rendering - physics));
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
            _scriptUpdate.Dispose();
            _scriptFixedUpdate.Dispose();
            _scriptLateUpdate.Dispose();
            _scriptDelayedTasks.Dispose();
            _cameraRender.Dispose();
            _renderPipeline.Dispose();
        }
    }
}
