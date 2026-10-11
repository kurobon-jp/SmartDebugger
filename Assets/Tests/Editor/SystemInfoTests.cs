using NUnit.Framework;
using UnityEngine;

namespace SmartDebugger.Tests
{
    public class SystemInfoTests
    {
        [Test]
        public void RuntimeStateReflectsTimeScaleChangesAfterFirstRead()
        {
            var previous = Time.timeScale;
            try
            {
                Time.timeScale = 1f;
                var first = SystemInfo.GetSystemInfoText()["Runtime State"];
                Time.timeScale = 0f;
                var second = SystemInfo.GetSystemInfoText()["Runtime State"];
                Assert.That(first, Does.Contain("Time Scale: 1"));
                Assert.That(second, Does.Contain("Time Scale: 0"));
            }
            finally
            {
                Time.timeScale = previous;
            }
        }

        [Test]
        public void FrameRateSettingsReflectChangesAfterFirstRead()
        {
            var previousTarget = Application.targetFrameRate;
            var previousVSync = QualitySettings.vSyncCount;
            try
            {
                Application.targetFrameRate = 30;
                QualitySettings.vSyncCount = 0;
                var first = SystemInfo.GetSystemInfoText()["Frame Rate Settings"];
                Application.targetFrameRate = 60;
                QualitySettings.vSyncCount = 1;
                var second = SystemInfo.GetSystemInfoText()["Frame Rate Settings"];
                Assert.That(first, Does.Contain("Target Frame Rate: 30").And.Contain("VSync Count: 0"));
                Assert.That(second, Does.Contain("Target Frame Rate: 60").And.Contain("VSync Count: 1"));
            }
            finally
            {
                Application.targetFrameRate = previousTarget;
                QualitySettings.vSyncCount = previousVSync;
            }
        }

        [Test]
        public void QualitySettingsReflectChangesAfterFirstRead()
        {
            var previous = QualitySettings.GetQualityLevel();
            if (QualitySettings.names.Length < 2) Assert.Ignore("Requires multiple quality levels.");
            try
            {
                QualitySettings.SetQualityLevel(0, false);
                var first = SystemInfo.GetSystemInfoText()["Rendering Settings"];
                QualitySettings.SetQualityLevel(1, false);
                var second = SystemInfo.GetSystemInfoText()["Rendering Settings"];
                Assert.That(first, Does.Contain($"Quality Level: {QualitySettings.names[0]} (0)"));
                Assert.That(second, Does.Contain($"Quality Level: {QualitySettings.names[1]} (1)"));
            }
            finally
            {
                QualitySettings.SetQualityLevel(previous, false);
            }
        }
    }
}
