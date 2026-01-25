using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmartDebugger
{
    public class FieldGroup
    {
        private string _title;
        private bool _foldout;
        private FieldGroupView _view;
        private readonly Action<FieldGroup> _builder;
        private readonly List<(IVariable variable, IFieldFactory factory)> _variables = new();

        public IReadOnlyList<(IVariable variable, IFieldFactory factory)> Variables => _variables;

        public string Title
        {
            get => _title;
            set => _title = value;
        }

        public bool Foldout
        {
            get => _foldout;
            set => _foldout = value;
        }

        internal FieldGroup(string title, bool foldout = true, Action<FieldGroup> builder = null)
        {
            Title = title;
            _foldout = foldout;
            _builder = builder;
        }

        public void AddField(IVariable variable, IFieldFactory factory)
        {
            _variables.Add((variable, factory));
        }

        internal void Build(Transform parent)
        {
            var prefab = SDSettings.Instance.LoadPrefab<FieldGroupView>("FieldGroup");
            _view = UnityEngine.Object.Instantiate(prefab, parent);
            Rebuild();
        }

        public void Rebuild()
        {
            if (_view == null) return;
            _variables.Clear();
            _builder?.Invoke(this);
            _view.Bind(this);
        }
    }

    public class FieldGroups
    {
        private readonly List<FieldGroup> _groups = new();

        public FieldGroup this[int index] => _groups[index];

        public int Count => _groups.Count;

        public void AddGroup(string title, Action<FieldGroup> builder)
        {
            _groups.Add(new FieldGroup(title, true, builder));
        }
    }
}