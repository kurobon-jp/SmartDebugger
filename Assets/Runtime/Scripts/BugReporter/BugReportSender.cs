using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartDebugger
{
    internal static class BugReportSender
    {
        internal static async Task<ReportResult> SendAsync(BugReporter reporter,
            string description, string report, byte[] screenshot, string videoPath,
            CancellationToken cancellationToken, TimeSpan timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var completion = new TaskCompletionSource<ReportResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            reporter.SendReport(description, report, screenshot, videoPath,
                result => completion.TrySetResult(result ?? ReportResult.Failed));
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
