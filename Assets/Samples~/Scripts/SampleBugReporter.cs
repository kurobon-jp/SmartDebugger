using System;
using System.Collections.Generic;
using System.IO;
using SmartDebugger;
using UnityEngine;

[CreateAssetMenu(fileName = "SampleBugReporter", menuName = "SmartDebugger/BugReporter/SampleBugReporter")]
public class SampleBugReporter : BugReporter
{
    public override string SendTo => "Save report";

    public override void SendReport(string description, string report, byte[] screenshot, Action<ReportResult> onResult)
    {
        var prefix = CreateFilePrefix();
        var reportUrls = new List<string>();
        var reportPath = Path.Combine(Application.persistentDataPath, $"{prefix}_BugReport.txt");
        var screenshotPath = Path.Combine(Application.persistentDataPath, $"{prefix}_Screenshot.png");
        if (!string.IsNullOrEmpty(report))
        {
            File.WriteAllText(reportPath, report);
            reportUrls.Add("file:///" + Uri.EscapeUriString(reportPath));
        }

        if (screenshot is { Length: > 0 })
        {
            File.WriteAllBytes(screenshotPath, screenshot);
            reportUrls.Add("file:///" + Uri.EscapeUriString(screenshotPath));
        }

        onResult(ReportResult.Success(reportUrls.ToArray()));
    }
}