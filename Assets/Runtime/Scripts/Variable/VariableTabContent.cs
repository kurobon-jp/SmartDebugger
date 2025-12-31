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

        private IFieldLayout _current;

        protected override void OnEnable()
        {
            base.OnEnable();
            Rebuild();
        }

        private void Rebuild()
        {
            var fieldLayouts = SmartDebug.Instance.FieldLayouts;
            var removedTabs = _tabs.Keys.Except(fieldLayouts).ToList();
            foreach (var fieldLayout in removedTabs)
            {
                _tabs.Remove(fieldLayout, out var tab);
                Destroy(tab.gameObject);
                if (_current == fieldLayout)
                {
                    _current = null;
                }
            }

            _current ??= fieldLayouts[0];
            for (var i = 0; i < fieldLayouts.Count; i++)
            {
                var fieldLayout = fieldLayouts[i];
                CreateTab(fieldLayout);
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