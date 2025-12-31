using System.Collections.Generic;
using UnityEngine;

namespace SmartDebugger
{
    public class SystemTabContent : MainTabContent
    {
        [SerializeField] private SystemInfoGroup _group;

        private readonly Dictionary<string, SystemInfoGroup> _groups = new();

        protected override void OnEnable()
        {
            Refresh();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            Refresh();
        }

        private void Refresh()
        {
            var values = SystemInfo.GetSystemInfoText();
            foreach (var value in values)
            {
                if (!_groups.TryGetValue(value.Key, out var group))
                {
                    var go = Instantiate(_group.gameObject, _group.transform.parent);
                    go.SetActive(true);
                    group = go.GetComponent<SystemInfoGroup>();
                    _groups.Add(value.Key, group);
                }

                group.SetText(value.Key, value.Value);
            }

            _group.gameObject.SetActive(false);
        }
    }
}