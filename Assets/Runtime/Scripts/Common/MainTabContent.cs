using UnityEngine;

namespace SmartDebugger
{
    public class MainTabContent : BaseView
    {
        [SerializeField] private string _title;
        [SerializeField] private Sprite _icon;
        
        public string Title => _title;
        public Sprite Icon => _icon;
    }
}