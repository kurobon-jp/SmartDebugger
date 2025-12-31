using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class FieldGroupView : BaseView
    {
        [SerializeField] private Text _title;
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _icon;
        [SerializeField] private Transform _fieldParent;

        private readonly List<GameObject> _fields = new();

        internal void Bind(FieldGroup group)
        {
            foreach (var field in _fields)
            {
                Destroy(field);
            }

            _fields.Clear();
            _title.text = group.Title;
            _toggle.SetIsOnWithoutNotify(group.Foldout);
            _fieldParent.gameObject.SetActive(group.Foldout);
            _icon.transform.localEulerAngles = Vector3.forward * (group.Foldout ? 0 : 90);
            foreach (var variable in group.Variables)
            {
                _fields.Add(variable.factory.Build(variable.variable, _fieldParent));
            }
        }

        public void OnToggleChanged(bool inOn)
        {
            _fieldParent.gameObject.SetActive(inOn);
            _icon.transform.localEulerAngles = Vector3.forward * (inOn ? 0 : 90);
        }
    }
}