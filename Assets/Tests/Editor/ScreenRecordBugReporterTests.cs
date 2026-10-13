using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SmartDebugger.Tests
{
    public class StubScreenRecordBugReporter : ScreenRecordBugReporter
    {
        public override string SendTo => "Test";
        public override void SendReport(string description, string report, byte[] screenshot, string videoPath,
            Action<ReportResult> onResult) => onResult(ReportResult.Success());
    }

    public class ScreenRecordBugReporterTests
    {
        private sealed class Recorder : IScreenRecorder
        {
            internal readonly TaskCompletionSource<string> Completion = new();
            internal int Exports;
            internal bool Disposed;
            public Task<string> StopAndExportAsync() { Exports++; return Completion.Task; }
            public void Dispose() => Disposed = true;
        }

        [UnityTest]
        public IEnumerator TimedOutExportRemainsOwnedAndAvailableForRetry()
        {
            var oldFactory = ScreenRecording.Create;
            var recorder = new Recorder();
            ScreenRecording.Create = _ => recorder;
            var reporter = ScriptableObject.CreateInstance<StubScreenRecordBugReporter>();
            var hadPreference = PlayerPrefs.HasKey("sd.include_screen_record");
            var preference = PlayerPrefs.GetInt("sd.include_screen_record");
            try
            {
                reporter.IncludeScreenRecord.Value = true;
                reporter.Initialize();
                var export = reporter.ExportForReportAsync();
                var wait = BugReportSender.WaitAsync(export, CancellationToken.None, TimeSpan.FromMilliseconds(10));
                while (!wait.IsCompleted) yield return null;
                Assert.That(wait.Exception?.GetBaseException(), Is.TypeOf<TimeoutException>());
                Assert.That(reporter.PendingExport, Is.SameAs(export));
                Assert.That(reporter.StopRecordingAndExportAsync(), Is.SameAs(export));
                Assert.That(recorder.Exports, Is.EqualTo(1));
                Assert.That(recorder.Disposed, Is.False);
                // Turning recording off must not dispose the detached exporter.
                reporter.IncludeScreenRecord.Value = false;
                Assert.That(recorder.Disposed, Is.False);
                recorder.Completion.SetResult("retained-video.mp4");
                while (!export.IsCompleted) yield return null;
                Assert.That(recorder.Disposed, Is.True);
                Assert.That(reporter.PendingExport, Is.Null);
                Assert.That(reporter.ExportForReportAsync().Result, Is.EqualTo("retained-video.mp4"));
                reporter.ClaimExport("retained-video.mp4");
                Assert.That(reporter.ExportForReportAsync().Result, Is.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(reporter);
                ScreenRecording.Create = oldFactory;
                if (hadPreference) PlayerPrefs.SetInt("sd.include_screen_record", preference);
                else PlayerPrefs.DeleteKey("sd.include_screen_record");
            }
        }
    }
}
