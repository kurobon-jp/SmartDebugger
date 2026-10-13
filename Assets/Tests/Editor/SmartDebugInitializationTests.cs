using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SmartDebugger.Tests
{
    public class SmartDebugInitializationTests
    {
        private static readonly FieldInfo DebugInstance = typeof(SmartDebug)
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo SettingsInstance = typeof(SDSettings)
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
        private SDSettings _settings;
        private object _previousDebugger;
        private object _previousSettings;

        [SetUp]
        public void SetUp()
        {
            _previousDebugger = DebugInstance.GetValue(null);
            _previousSettings = SettingsInstance.GetValue(null);
            var path = "Packages/com.github.kurobon.smart_debugger/Runtime/Settings/SDSettings.asset";
            var source = AssetDatabase.LoadAssetAtPath<SDSettings>(path)
                ?? AssetDatabase.LoadAssetAtPath<SDSettings>("Assets/Runtime/Settings/SDSettings.asset");
            Assert.That(source, Is.Not.Null);
            _settings = UnityEngine.Object.Instantiate(source);
            SetSetting("_autoInitialize", false);
            SetSetting("_dontDestroyOnLoad", false);
            SetSetting("_autoGenerateEventSystem", false);
            DebugInstance.SetValue(null, null);
            SettingsInstance.SetValue(null, _settings);
        }

        private void SetSetting(string name, bool value) => typeof(SDSettings)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_settings, value);

        [TearDown]
        public void TearDown()
        {
            var created = DebugInstance.GetValue(null) as SmartDebug;
            if (created != null) UnityEngine.Object.DestroyImmediate(created.gameObject);
            UnityEngine.Object.DestroyImmediate(_settings);
            DebugInstance.SetValue(null, _previousDebugger);
            SettingsInstance.SetValue(null, _previousSettings);
        }

        [Test]
        public void InstanceDoesNotInitializeWhenAutoInitializeIsDisabled()
        {
            Assert.Throws<InvalidOperationException>(() => { _ = SmartDebug.Instance; });
            Assert.That(DebugInstance.GetValue(null), Is.Null);
        }

        [Test]
        public void ExplicitInitializationWorksWhenAutoInitializeIsDisabled()
        {
            SmartDebug.Initialize();
            var initialized = DebugInstance.GetValue(null);
            Assert.That(initialized, Is.Not.Null);
            Assert.That(SmartDebug.Instance, Is.SameAs(initialized));
            SmartDebug.Initialize();
            Assert.That(SmartDebug.Instance, Is.SameAs(initialized));
        }

        [Test]
        public void InstanceInitializesWhenAutoInitializeIsEnabled()
        {
            SetSetting("_autoInitialize", true);
            Assert.That(SmartDebug.Instance, Is.Not.Null);
        }
    }
}
