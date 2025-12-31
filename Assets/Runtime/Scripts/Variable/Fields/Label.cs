using UnityEngine;
using UnityEngine.UI;

namespace SmartDebugger
{
    public class Label : BaseField
    {
        [SerializeField] private Text _title;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Bind(IVariable variable)
        {
            _title.text = variable.Title;
        }
    }
}