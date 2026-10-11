using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor.Compilation;

public class PackageVerificationTests
{
    [Test]
    public void SampleReporterUsesVersionAppropriateBase()
    {
#if DISABLE_SMART_DEBUG
        Assert.That(CompilationPipeline.GetAssemblies(AssembliesType.Player)
            .Any(assembly => assembly.name == "Unity.SmartDebugger.Samples"), Is.False);
#else
        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .Single(item => item.GetName().Name == "Unity.SmartDebugger.Samples");
        var reporter = assembly.GetType("SampleBugReporter", true);
#if UNITY_2022_3_OR_NEWER
        Assert.That(reporter.BaseType.FullName, Is.EqualTo("SmartDebugger.ScreenRecordBugReporter"));
#else
        Assert.That(reporter.BaseType.FullName, Is.EqualTo("SmartDebugger.BugReporter"));
#endif
#endif
    }

    [Test]
    public void RuntimeAssembliesMatchConsumerConfiguration()
    {
        var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);
        var names = assemblies.Select(assembly => assembly.name).ToArray();
        var variant = File.ReadAllText("verification-variant.txt").Trim();
#if DISABLE_SMART_DEBUG
        Assert.That(variant, Is.EqualTo("disabled"));
        Assert.That(names.Any(name => name.StartsWith("Unity.SmartDebugger", StringComparison.Ordinal)), Is.False);
#else
        Assert.That(variant, Is.Not.EqualTo("disabled"), "DISABLE_SMART_DEBUG must be active.");
        Assert.That(names, Does.Contain("Unity.SmartDebugger"));
#if SMART_DEBUG_VERIFY_MINIMAL
        Assert.That(variant, Is.EqualTo("minimal"));
        Assert.That(names, Does.Not.Contain("Unity.SmartDebugger.InputSystem"));
        Assert.That(names, Does.Not.Contain("Unity.SmartDebugger.InstantReplay"));
#endif
#if SMART_DEBUG_VERIFY_INPUT_SYSTEM || SMART_DEBUG_VERIFY_RECORDING
        Assert.That(names, Does.Contain("Unity.SmartDebugger.InputSystem"));
#endif
#if SMART_DEBUG_VERIFY_RECORDING
        Assert.That(names, Does.Contain("Unity.SmartDebugger.InstantReplay"));
#endif
#endif
    }
}
