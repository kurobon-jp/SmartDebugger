using System;
using UnityEngine;

namespace SmartDebugger
{
    public abstract class BugReporter : ScriptableObject
    {
        public BoolVariable IncludeLogs { get; } =
            new("IncludeLogs", true, serializeKey: "sd.include_logs");

        public BoolVariable IncludeScreenCapture { get; } =
            new("IncludeScreenCapture", true, serializeKey: "sd.include_screen_capture");

        public BoolVariable IncludeScreenRecord { get; } =
            new("IncludeScreenRecord", serializeKey: "sd.include_screen_record");
        
        public abstract string SendTo { get; }

        internal virtual void Initialize()
        {
        }

        public abstract void SendReport(string description, string report, byte[] screenshot, string videoPath,
            Action<ReportResult> onResult);

        protected static string CreateFilePrefix()
        {
            var timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var userId = Environment.UserName;
            if (string.IsNullOrEmpty(userId) || userId == "Unknown")
            {
                userId = UnityEngine.SystemInfo.deviceUniqueIdentifier;
            }

            return $"{userId}_{timestamp}";
        }
    }

    public class ReportResult
    {
        public static readonly ReportResult Failed = new(false);

        public bool IsSuccess { get; }
        public string[] ReportUrls { get; }

        public static ReportResult Success(params string[] reportUrls)
        {
            return new ReportResult(true, reportUrls);
        }

        private ReportResult(bool success, params string[] reportUrls)
        {
            IsSuccess = success;
            ReportUrls = reportUrls;
        }
    }
}