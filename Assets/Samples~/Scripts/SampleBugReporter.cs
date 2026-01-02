using System;
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
        var reportPath = Path.Combine(Application.persistentDataPath, $"{prefix}_BugReport.txt");
        var screenshotPath = Path.Combine(Application.persistentDataPath, $"{prefix}_Screenshot.png");
        File.WriteAllText(reportPath, report);
        File.WriteAllBytes(screenshotPath, screenshot);
        onResult(ReportResult.Success(
            "file://" + Uri.EscapeUriString(reportPath), 
            "file://" + Uri.EscapeUriString(screenshotPath)));
    }
}