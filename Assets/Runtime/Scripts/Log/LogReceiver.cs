using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmartDebugger
{
    public class LogReceiver : IDisposable
    {
        private readonly object _queueLock = new();
        private Queue<LogEntry> _queue = new();
        private Queue<LogEntry> _processing = new();
        private readonly Queue<LogEntry> _retained = new();
        private readonly List<LogEntry> _filtered = new();
        private readonly int _maxEntries;
        private bool _disposed;
        private int _ids;
        private LogTypes _filterTypes;

        public int Count => _filtered.Count;
        public int InfoCount { get; private set; }
        public int WarnCount { get; private set; }
        public int ErrorCount { get; private set; }
        public string FilterText { get; private set; }

        public bool IsInfoFilter => (_filterTypes & LogTypes.Info) == LogTypes.Info;
        public bool IsWarnFilter => (_filterTypes & LogTypes.Warning) == LogTypes.Warning;
        public bool IsErrorFilter => (_filterTypes & LogTypes.Error) == LogTypes.Error;

        public Dictionary<int, LogEntry> Entries { get; } = new();

        internal event Action<LogEntry> OnAdded;

        internal LogReceiver(int maxEntries = 1000)
        {
            if (maxEntries < 1) throw new ArgumentOutOfRangeException(nameof(maxEntries));
            _maxEntries = maxEntries;
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            lock (_queueLock)
            {
                if (_disposed) return;
                if (_queue.Count >= _maxEntries) _queue.Dequeue();
                _queue.Enqueue(new LogEntry(_ids++, condition, stackTrace, type));
            }
        }

        public void Update()
        {
            lock (_queueLock)
            {
                if (_disposed) return;
                // A subscriber may have interrupted the previous batch. Drain
                // its remaining entries before swapping in newer received logs.
                if (_processing.Count == 0)
                {
                    if (_queue.Count == 0) return;
                    (_queue, _processing) = (_processing, _queue);
                }
            }

            // Invoke subscribers outside the lock so logging from them cannot
            // modify the batch being drained or block worker-thread logging.
            while (_processing.Count > 0 && !_disposed)
            {
                var entry = _processing.Dequeue();
                AddEntry(entry);
                OnAdded?.Invoke(entry);
            }
        }

        public LogEntry FindById(int id)
        {
            return Entries[id];
        }

        public LogEntry FindByIndex(int index)
        {
            return _filtered[index];
        }

        private void AddEntry(LogEntry entry)
        {
            if (_retained.Count >= _maxEntries)
            {
                var oldest = _retained.Dequeue();
                Entries.Remove(oldest.Id);
                if (_filtered.Count > 0 && _filtered[0].Id == oldest.Id) _filtered.RemoveAt(0);
                UpdateTypeCount(oldest.Types, -1);
            }

            _retained.Enqueue(entry);
            Entries.Add(entry.Id, entry);
            UpdateTypeCount(entry.Types, 1);
            ApplyFilter(entry);
        }

        private void UpdateTypeCount(LogTypes type, int change)
        {
            switch (type)
            {
                case LogTypes.Info:
                    InfoCount += change;
                    break;
                case LogTypes.Warning:
                    WarnCount += change;
                    break;
                case LogTypes.Error:
                    ErrorCount += change;
                    break;
            }
        }

        private void ApplyFilter(LogEntry entry)
        {
            // The mask contains hidden log types, not selected visible types.
            var matchesType = (_filterTypes & entry.Types) == 0;
            var matchesText = string.IsNullOrEmpty(FilterText) || entry.Message.Contains(FilterText, StringComparison.OrdinalIgnoreCase);

            if (matchesType && matchesText)
            {
                _filtered.Add(entry);
            }
        }

        public void Clear()
        {
            lock (_queueLock)
            {
                _queue.Clear();
            }
            _processing.Clear();
            _retained.Clear();
            _filtered.Clear();
            Entries.Clear();
            InfoCount = 0;
            WarnCount = 0;
            ErrorCount = 0;
        }

        public void Filter(LogTypes types, string text)
        {
            if (_filterTypes == types && FilterText == text) return;
            _filtered.Clear();
            _filterTypes = types;
            FilterText = text;
            foreach (var entry in _retained)
            {
                ApplyFilter(entry);
            }
        }

        public void Dispose()
        {
            lock (_queueLock)
            {
                if (_disposed) return;
                _disposed = true;
                Application.logMessageReceivedThreaded -= OnLogMessageReceived;
                _queue.Clear();
            }

            Clear();
            OnAdded = null;
        }
    }
}
