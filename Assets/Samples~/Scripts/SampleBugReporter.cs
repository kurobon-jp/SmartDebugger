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
        var reportPath = Path.Combine(Application.persistentDataPath, "BugReport.txt");
        var screenshotPath = Path.Combine(Application.persistentDataPath, "Screenshot.png");
        File.WriteAllText(reportPath, report);
        File.WriteAllBytes(screenshotPath, screenshot);
        onResult(ReportResult.Success(
            new Uri(Uri.EscapeUriString(reportPath)),
            new Uri(Uri.EscapeUriString(screenshotPath))
        ));
    }
}