using System;
using UnityEngine;

namespace SmartDebugger
{
    public abstract class BugReporter : ScriptableObject
    {
        public abstract string SendTo { get; }

        public abstract void SendReport(string description, string report, byte[] screenshot,
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

        public bool Success { get; }
        public string ReportUrl { get; }

        public ReportResult(bool success, string reportUrl = null)
        {
            Success = success;
            ReportUrl = reportUrl;
        }
    }
}