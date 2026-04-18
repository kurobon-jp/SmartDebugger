using System.Linq;
using UnityEngine;

namespace SmartDebugger
{
    public class SystemTabContent : MainTabContent
    {
        [SerializeField] private SystemInfoGroup _group;

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
            var systemInfos = SystemInfo.GetSystemInfoText();
            var keys = systemInfos.Keys.ToList();
            _group.PooledInstantiate(keys.Count, (i, group) =>
            {
                var key = keys[i];
                group.Bind(key, systemInfos[key]);
            });
        }
    }
}