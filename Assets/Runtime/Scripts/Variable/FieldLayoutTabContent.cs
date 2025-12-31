using UnityEngine;

namespace SmartDebugger
{
    public class FieldLayoutTabContent : BaseView
    {
        [SerializeField] private Transform _groupParent;

        private IFieldLayout _fieldLayout;

        public void Bind(IFieldLayout fieldLayout)
        {
            var groups = new FieldGroups();
            fieldLayout.OnLayout(groups);
            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                group.Build(_groupParent);
            }
        }
    }
}