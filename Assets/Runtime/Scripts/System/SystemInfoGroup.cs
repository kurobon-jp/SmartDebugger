using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class SystemInfoGroup : BaseView
    {
        [SerializeField] private Text _title;
        [SerializeField] private Text _value;

        public void Bind(string title, string value)
        {
            _title.text = title;
            _value.text = value;
        }
    }
}