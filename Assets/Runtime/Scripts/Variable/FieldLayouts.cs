using System.Collections;
using System.Collections.Generic;

namespace SmartDebugger
{
    internal class FieldLayouts : IEnumerable<IFieldLayout>
    {
        private readonly List<IFieldLayout> _fieldLayouts = new();

        internal int Count => _fieldLayouts.Count;
        internal IFieldLayout this[int index] => _fieldLayouts[index];
        internal bool IsDirty { get; set; }

        internal void Add(IFieldLayout fieldLayout)
        {
            _fieldLayouts.Add(fieldLayout);
            IsDirty = true;
        }

        internal void Remove(IFieldLayout fieldLayout)
        {
            _fieldLayouts.Remove(fieldLayout);
            IsDirty = true;
        }

        public IEnumerator<IFieldLayout> GetEnumerator()
        {
            return _fieldLayouts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}