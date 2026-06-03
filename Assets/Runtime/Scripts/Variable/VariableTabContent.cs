using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmartDebugger
{
    public class VariableTabContent : MainTabContent
    {
        [SerializeField] private RectTransform _content;
        [SerializeField] private FieldLayoutTab _tab;

        private readonly Dictionary<IFieldLayout, FieldLayoutTab> _tabs = new();

        private FieldLayouts _fieldLayouts;
        private IFieldLayout _current;

        protected override void OnEnable()
        {
            _fieldLayouts = SmartDebug.Instance.FieldLayouts;
            Rebuild();
        }

        private void Update()
        {
            if (_fieldLayouts.IsDirty)
            {
                Rebuild();
            }
        }

        private void Rebuild()
        {
            _fieldLayouts.IsDirty = false;
            var removedTabs = _tabs.Keys.Except(_fieldLayouts).ToList();
            foreach (var fieldLayout in removedTabs)
            {
                _tabs.Remove(fieldLayout, out var tab);
                Destroy(tab.gameObject);
                if (_current == fieldLayout)
                {
                    _current = null;
                }
            }

            if (_fieldLayouts.Count > 0)
            {
                _current ??= _fieldLayouts[0];
                foreach (var fieldLayout in _fieldLayouts)
                {
                    CreateTab(fieldLayout);
                }
            }

            _tab.gameObject.SetActive(false);
        }

        private void CreateTab(IFieldLayout fieldLayout)
        {
            if (_tabs.TryGetValue(fieldLayout, out var tab)) return;
            tab = Instantiate(_tab, _tab.transform.parent);
            tab.Bind(fieldLayout, _content);
            _tabs[fieldLayout] = tab;
        }
    }
}