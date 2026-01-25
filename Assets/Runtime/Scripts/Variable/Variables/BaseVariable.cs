using System;
using UnityEngine;

namespace SmartDebugger
{
    public interface IVariable
    {
        string Title { get; }
        bool Interactable { get; }
    }

    public interface IFieldFactory
    {
        GameObject Build(IVariable variable, Transform parent);
    }

    public abstract class BaseVariable : IVariable
    {
        private string _title;
        private bool _interactable = true;

        public event Action<bool> OnInteractabilityChanged;

        public string Title
        {
            get => _title;
            set => _title = value;
        }

        public bool Interactable
        {
            get => _interactable;
            set
            {
                _interactable = value;
                OnInteractabilityChanged?.Invoke(value);
            }
        }

        protected BaseVariable(string title, bool interactable = true)
        {
            Title = title;
            Interactable = interactable;
        }
    }

    public abstract class SerializeVariable<T> : BaseVariable
    {
        private T _value;
        private bool _deserialized;
        private readonly T _defaultValue;

        public T Value
        {
            get
            {
                if (_deserialized) return _value;
                _deserialized = true;
                SetDefaultValue(_defaultValue);
                return _value;
            }
            set
            {
                var newValue = Validate(value);
                if (Equals(_value, newValue)) return;
                SetValue(newValue);
            }
        }

        protected string SerializeKey { get; private set; }

        public virtual string TextValue => Value == null ? string.Empty : Value.ToString();

        public event Action<SerializeVariable<T>> OnValueChanged;

        protected SerializeVariable(string title, T defaultValue = default, string serializeKey = null,
            bool interactable = true) : base(title, interactable)
        {
            _defaultValue = defaultValue;
            SerializeKey = serializeKey;
        }

        private void SetDefaultValue(T value)
        {
            value = GetSerializeValue(value);
            SetValue(value, false, false);
        }

        private protected virtual T Validate(T value)
        {
            return value;
        }

        private void SetValue(T value, bool notify = true, bool serialize = true)
        {
            _value = value;
            if (notify)
            {
                OnValueChanged?.Invoke(this);
            }

            if (serialize)
            {
                SetSerializeValue(_value);
            }
        }

        private protected abstract void SetSerializeValue(T value);
        private protected abstract T GetSerializeValue(T defaultValue);
    }
}