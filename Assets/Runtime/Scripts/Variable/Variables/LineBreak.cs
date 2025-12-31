using UnityEngine;

namespace SmartDebugger
{
    public class LineBreak : IVariable
    {
        public string Title { get; }
        public bool Interactable { get; }
        
        public class Factory : IFieldFactory
        {
            public GameObject Build(IVariable variable, Transform parent)
            {
                var prefab = Resources.Load<GameObject>("Prefabs/Fields/LineBreak");
                return Object.Instantiate(prefab, parent);
            }
        }
    }
}