using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmartDebugger
{
    public class LogReceiver
    {
        private readonly List<LogEntry> _filtered = new();
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

        public event Action OnAdding;
        public event Action OnAdded;

        internal LogReceiver()
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            OnAdding?.Invoke();
            AddEntry(new LogEntry(_ids++, condition, stackTrace, type, Time.time));
            OnAdded?.Invoke();
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
            Entries.Add(entry.Id, entry);
            switch (entry.Types)
            {
                case LogTypes.Info:
                    InfoCount++;
                    break;
                case LogTypes.Warning:
                    WarnCount++;
                    break;
                case LogTypes.Error:
                    ErrorCount++;
                    break;
            }

            ApplyFilter(entry);
        }

        private void ApplyFilter(LogEntry entry)
        {
            if ((_filterTypes & entry.Types) == 0 &&
                (string.IsNullOrEmpty(FilterText) ||
                 entry.Message.Contains(FilterText, StringComparison.OrdinalIgnoreCase)))
            {
                _filtered.Add(entry);
            }
        }

        public void Clear()
        {
            Entries.Clear();
            _filtered.Clear();
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
            foreach (var entry in Entries.Values)
            {
                ApplyFilter(entry);
            }
        }
    }
}