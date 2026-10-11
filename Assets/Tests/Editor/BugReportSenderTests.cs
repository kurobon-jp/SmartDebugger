using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SmartDebugger.Tests
{
    public class StubBugReporter : BugReporter
    {
        public Action<Action<ReportResult>> Send;
        public override string SendTo => "Test";
        public override void SendReport(string description, string report, byte[] screenshot, string videoPath,
            Action<ReportResult> onResult) => Send(onResult);
    }

    public class BugReportSenderTests
    {
        private StubBugReporter _reporter;

        [SetUp]
        public void SetUp() => _reporter = ScriptableObject.CreateInstance<StubBugReporter>();

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(_reporter);

        private Task<ReportResult> Send(CancellationToken token = default)
            => BugReportSender.SendAsync(_reporter, "test", "", null, null, token, TimeSpan.FromMilliseconds(30));

        private static IEnumerator Wait(Task task)
        {
            var deadline = DateTime.UtcNow.AddSeconds(5);
            while (!task.IsCompleted && DateTime.UtcNow < deadline) yield return null;
            Assert.That(task.IsCompleted, Is.True, "Submission did not settle.");
        }

        [UnityTest]
        public IEnumerator DuplicateCallbacksKeepFirstResult()
        {
            _reporter.Send = callback =>
            {
                callback(ReportResult.Success("test-url"));
                callback(ReportResult.Failed);
            };
            var task = Send();
            yield return Wait(task);
            Assert.That(task.Result.IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator SynchronousReporterExceptionIsObservable()
        {
            _reporter.Send = _ => throw new InvalidOperationException("test failure");
            var task = Send();
            yield return Wait(task);
            Assert.That(task.Exception?.GetBaseException(), Is.TypeOf<InvalidOperationException>());
        }

        [UnityTest]
        public IEnumerator MissingCallbackTimesOut()
        {
            _reporter.Send = _ => { };
            var task = Send();
            yield return Wait(task);
            Assert.That(task.Exception?.GetBaseException(), Is.TypeOf<TimeoutException>());
        }

        [UnityTest]
        public IEnumerator CancellationIgnoresLateCallback()
        {
            Action<ReportResult> callback = null;
            _reporter.Send = result => callback = result;
            using var cancellation = new CancellationTokenSource();
            var task = Send(cancellation.Token);
            cancellation.Cancel();
            yield return Wait(task);
            callback(ReportResult.Success());
            Assert.That(task.IsCanceled, Is.True);
        }
    }
}
