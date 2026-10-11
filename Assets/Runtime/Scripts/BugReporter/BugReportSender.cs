using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace SmartDebugger
{
    internal static class BugReportSender
    {
        private sealed class Submission
        {
            internal Task<ReportResult> Pending;
        }

        private static readonly ConditionalWeakTable<BugReporter, Submission> Submissions = new();

        internal static Task PendingSubmission(BugReporter reporter)
        {
            if (!Submissions.TryGetValue(reporter, out var submission)) return null;
            lock (submission) return submission.Pending is { IsCompleted: false } ? submission.Pending : null;
        }

        internal static async Task<ReportResult> SendAsync(BugReporter reporter,
            string description, string report, byte[] screenshot, string videoPath,
            CancellationToken cancellationToken, TimeSpan timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var completion = new TaskCompletionSource<ReportResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            var submission = Submissions.GetValue(reporter, _ => new Submission());
            lock (submission)
            {
                if (submission.Pending is { IsCompleted: false })
                    throw new InvalidOperationException("A report submission is still running.");
                submission.Pending = completion.Task;
            }
            try
            {
                reporter.SendReport(description, report, screenshot, videoPath,
                    result => completion.TrySetResult(result ?? ReportResult.Failed));
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }
            // Timeout/cancellation ends the caller's wait, not provider ownership.
            return await WaitAsync(completion.Task, cancellationToken, timeout);
        }

        internal static async Task<T> WaitAsync<T>(Task<T> task, CancellationToken cancellationToken, TimeSpan timeout)
        {
            using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            try
            {
                var delay = Task.Delay(timeout, timeoutCancellation.Token);
                if (await Task.WhenAny(task, delay) != task)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new TimeoutException("Bug report operation timed out.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                return await task;
            }
            finally
            {
                timeoutCancellation.Cancel();
            }
        }
    }
}
