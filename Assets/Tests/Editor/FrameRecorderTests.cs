using System.Collections;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TestTools;

namespace SmartDebugger.Tests
{
    public class FrameRecorderTests
    {
        [UnityTest]
        public IEnumerator UpdateLoadIsRecordedAsScripts()
        {
            yield return new EnterPlayMode();
            var wasProfiling = UnityEngine.Profiling.Profiler.enabled;
            UnityEngine.Profiling.Profiler.enabled = false;
            var load = new GameObject("FrameRecorder script load");
            var probe = load.AddComponent<FrameRecorderScriptLoad>();
            float scripts;
            using (var recorder = new FrameRecorder())
            {
                probe.Recorder = recorder;
                for (var i = 0; i < 5; i++) yield return null;
                scripts = probe.Scripts;
                probe.Recorder = null;
            }
            Object.Destroy(load);
            UnityEngine.Profiling.Profiler.enabled = wasProfiling;
            yield return new ExitPlayMode();
            Assert.That(scripts, Is.GreaterThan(5f), "15ms of Update work must not be classified as Other.");
        }

        [Test]
        public void CategoriesUseMeasuredWorkAndLeaveWaitsInOther()
        {
            var sample = FrameRecorder.ComposeCpuSamples(16f, 2f, 3f, 1f);
            Assert.That(sample, Is.EqualTo(new Vector4(2f, 3f, 1f, 10f)));
        }

        [Test]
        public void MissingMarkersRemainUnclassified()
        {
            Assert.That(FrameRecorder.ComposeCpuSamples(16f, 0f, 0f, 0f),
                Is.EqualTo(new Vector4(0f, 0f, 0f, 16f)));
        }

        [UnityTest]
        public IEnumerator MarkerRecorderSumsMainThreadCallsAndExcludesWorkerCalls()
        {
            var marker = new ProfilerMarker("SmartDebugger.Tests.ThreadTiming");
            var factory = typeof(FrameRecorder).GetMethod("RecordMainThread",
                BindingFlags.NonPublic | BindingFlags.Static);
            using (var recorder = (ProfilerRecorder)factory.Invoke(null,
                       new object[] { "SmartDebugger.Tests.ThreadTiming" }))
            {
                using (marker.Auto()) { }
                using (marker.Auto()) { }
                var worker = new Thread(() => { using (marker.Auto()) { } });
                worker.Start();
                worker.Join();
                yield return null;
                Assert.That(recorder.Count, Is.GreaterThan(0));
                Assert.That(recorder.GetSample(0).Count, Is.EqualTo(2));
            }
        }

        [Test]
        public void PlayerLoopMarkersAreAvailable()
        {
            using (var recorder = new FrameRecorder())
            {
                foreach (var name in new[] { "_scriptUpdate", "_scriptFixedUpdate", "_scriptLateUpdate",
                             "_scriptDelayedTasks", "_physicsProcessing", "_physics2DSimulate", "_cameraRender" })
                {
                    var field = typeof(FrameRecorder).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
                    Assert.That(((ProfilerRecorder)field.GetValue(recorder)).Valid, Is.True, name);
                }
            }
        }
    }

    public class FrameRecorderScriptLoad : MonoBehaviour
    {
        internal FrameRecorder Recorder;
        internal float Scripts;
        private void Update() => Thread.Sleep(15);
        private void LateUpdate()
        {
            if (Recorder == null) return;
            Recorder.Sample();
            Scripts = Recorder.CpuSamples[Recorder.SampleOffset].x;
        }
    }
}
