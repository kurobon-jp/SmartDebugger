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

    public interface IRefreshable
    {
        void Refresh();
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

    public abstract class SerializeVariable<T> : BaseVariable, IRefreshable
    {
        private T _value;
        private bool _deserialized;
        private readonly T _defaultValue;
        private readonly Func<T> _getter;

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

        protected SerializeVariable(string title, Func<T> getter, string serializeKey = null,
            bool interactable = true) : base(title, interactable)
        {
            _getter = getter;
            _defaultValue = getter.Invoke();
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

        public static implicit operator T(SerializeVariable<T> variable)
        {
            return variable.Value;
        }

        private protected abstract void SetSerializeValue(T value);

        private protected abstract T GetSerializeValue(T defaultValue);

        void IRefreshable.Refresh()
        {
            if (_getter == null) return;
            Value = _getter.Invoke();
        }
    }
}