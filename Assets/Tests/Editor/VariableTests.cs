using NUnit.Framework;
using UnityEngine;

namespace SmartDebugger.Tests
{
    public class VariableTests
    {
        [Test]
        public void WriteBeforeFirstReadKeepsAssignedValue()
        {
            var variable = new IntVariable("test", defaultValue: 50);
            variable.Value = 20;
            Assert.That(variable.Value, Is.EqualTo(20));
        }

        [Test]
        public void AssigningZeroBeforeFirstReadOverridesNonzeroDefault()
        {
            var variable = new IntVariable("test", defaultValue: 50);
            variable.Value = 0;
            Assert.That(variable.Value, Is.Zero);
        }

        [Test]
        public void InitialWriteValidatesAndNotifiesOnce()
        {
            var variable = new IntVariable("test", defaultValue: 50, maxValue: 100);
            var notifications = 0;
            variable.OnValueChanged += _ => notifications++;
            variable.Value = 200;
            Assert.That(variable.Value, Is.EqualTo(100));
            Assert.That(notifications, Is.EqualTo(1));
        }

        [Test]
        public void InitialWriteReplacesPersistedValue()
        {
            const string key = "sd.tests.initial_write";
            try
            {
                PlayerPrefs.SetInt(key, 80);
                var variable = new IntVariable("test", defaultValue: 50, serializeKey: key);
                variable.Value = 0;
                Assert.That(variable.Value, Is.Zero);
                Assert.That(PlayerPrefs.GetInt(key), Is.Zero);
            }
            finally
            {
                PlayerPrefs.DeleteKey(key);
            }
        }

        [Test]
        public void InitialFalseWriteOverridesTrueDefault()
        {
            var variable = new BoolVariable("test", true);
            variable.Value = false;
            Assert.That(variable.Value, Is.False);
        }
    }
}
