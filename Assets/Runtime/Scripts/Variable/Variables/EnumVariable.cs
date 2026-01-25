using System;

namespace SmartDebugger
{
    public class EnumVariable<T> : SelectionVariable where T : Enum
    {
        public T EnumValue
        {
            get => (T)Enum.GetValues(typeof(T)).GetValue(Value);
            set => Value = Array.IndexOf(Enum.GetValues(typeof(T)), value);
        }

        public EnumVariable(string title,
            T defaultValue = default,
            string serializeKey = null,
            bool interactable = true
        ) : base(
            title,
            () => Enum.GetNames(typeof(T)), Array.IndexOf(Enum.GetValues(typeof(T)), defaultValue),
            serializeKey,
            interactable)
        {
        }
    }
}