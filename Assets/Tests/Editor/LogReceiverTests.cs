using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace SmartDebugger.Tests
{
    public class LogReceiverTests
    {
        private LogReceiver _receiver;
        private static readonly MethodInfo Receive = typeof(LogReceiver).GetMethod(
            "OnLogMessageReceived", BindingFlags.Instance | BindingFlags.NonPublic);

        [SetUp]
        public void SetUp() => _receiver = new LogReceiver(3);

        [TearDown]
        public void TearDown() => _receiver.Dispose();

        private void Log(string message, LogType type = LogType.Log)
            => Receive.Invoke(_receiver, new object[] { message, "", type });

        [Test]
        public void PendingQueueKeepsOnlyNewestEntries()
        {
            for (var i = 0; i < 10; i++) Log(i.ToString());
            _receiver.Update();
            Assert.That(_receiver.Entries.Count, Is.EqualTo(3));
            Assert.That(_receiver.FindByIndex(0).Message, Is.EqualTo("7"));
            Assert.That(_receiver.FindByIndex(2).Message, Is.EqualTo("9"));
        }

        [Test]
        public void EvictionUpdatesFiltersAndCounters()
        {
            Log("old warning", LogType.Warning);
            Log("keep error", LogType.Error);
            Log("other");
            _receiver.Update();
            _receiver.Filter(LogTypes.Info, "keep");
            Log("keep warning", LogType.Warning);
            _receiver.Update();
            Assert.That(_receiver.Entries.ContainsKey(0), Is.False);
            Assert.That(_receiver.WarnCount, Is.EqualTo(1));
            Assert.That(_receiver.ErrorCount, Is.EqualTo(1));
            Assert.That(_receiver.InfoCount, Is.EqualTo(1));
            Assert.That(_receiver.Count, Is.EqualTo(2));
            _receiver.Filter(LogTypes.None, "");
            Assert.That(_receiver.FindByIndex(0).Message, Is.EqualTo("keep error"));
        }

        [Test]
        public void LoggingFromSubscriberIsProcessedOnNextUpdate()
        {
            _receiver.OnAdded += entry =>
            {
                if (entry.Message == "first") Log("second");
            };
            Log("first");
            _receiver.Update();
            Assert.That(_receiver.Count, Is.EqualTo(1));
            _receiver.Update();
            Assert.That(_receiver.Count, Is.EqualTo(2));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SubscriberExceptionPreservesPendingBatchOrder(bool receiveNewLog)
        {
            var received = new System.Collections.Generic.List<string>();
            _receiver.OnAdded += entry =>
            {
                received.Add(entry.Message);
                if (entry.Message == "first") throw new InvalidOperationException("subscriber failure");
            };
            Log("first");
            Log("second");
            Assert.Throws<InvalidOperationException>(() => _receiver.Update());
            if (receiveNewLog) Log("third");

            _receiver.Update();
            Assert.That(received, Is.EqualTo(new[] { "first", "second" }));
            if (receiveNewLog)
            {
                _receiver.Update();
                Assert.That(received, Is.EqualTo(new[] { "first", "second", "third" }));
            }
            Assert.That(_receiver.FindByIndex(0).Message, Is.EqualTo("first"));
            Assert.That(_receiver.FindByIndex(1).Message, Is.EqualTo("second"));
        }

        [Test]
        public void SubscriberExceptionPreservesEvictionFiltersAndCounters()
        {
            _receiver.Filter(LogTypes.Info, "keep");
            _receiver.OnAdded += entry =>
            {
                if (entry.Message == "first") throw new InvalidOperationException("subscriber failure");
            };
            Log("first");
            Log("keep warning", LogType.Warning);
            Log("keep error", LogType.Error);
            Assert.Throws<InvalidOperationException>(() => _receiver.Update());
            Log("new info");
            _receiver.Update();
            _receiver.Update();

            Assert.That(_receiver.Entries.ContainsKey(0), Is.False);
            Assert.That(_receiver.Entries.Count, Is.EqualTo(3));
            Assert.That(_receiver.InfoCount, Is.EqualTo(1));
            Assert.That(_receiver.WarnCount, Is.EqualTo(1));
            Assert.That(_receiver.ErrorCount, Is.EqualTo(1));
            Assert.That(_receiver.Count, Is.EqualTo(2));
            Assert.That(_receiver.FindByIndex(0).Message, Is.EqualTo("keep warning"));
            Assert.That(_receiver.FindByIndex(1).Message, Is.EqualTo("keep error"));
            _receiver.Filter(LogTypes.None, "");
            Assert.That(_receiver.FindByIndex(2).Message, Is.EqualTo("new info"));
        }

        [Test]
        public void ClearAlsoDiscardsPendingLogs()
        {
            Log("pending");
            _receiver.Clear();
            _receiver.Update();
            Assert.That(_receiver.Count, Is.Zero);
        }

        [Test]
        public void DisposeStopsReceivingAndIsIdempotent()
        {
            _receiver.Dispose();
            _receiver.Dispose();
            Debug.Log("SmartDebugger disposed receiver test");
            Log("late");
            _receiver.Update();
            Assert.That(_receiver.Entries, Is.Empty);
        }
    }
}
