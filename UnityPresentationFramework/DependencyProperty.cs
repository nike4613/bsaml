using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace UnityPresentationFramework
{
    public delegate bool ValidateValue<in TOwner, in T>(TOwner owner, T value) where TOwner : DependencyObject;
    public delegate void PropertyChanged<in TOwner, in T>(TOwner owner, T value) where TOwner : DependencyObject;

    public abstract class DependencyProperty
    {
        public static DependencyProperty<TValueType> Register<TOwningType, TValueType>(
                string name,
                TValueType defaultValue,
                PropertyChanged<TOwningType, TValueType>? onChange = null,
                ValidateValue<TOwningType, TValueType>? validate = null
            )
            where TOwningType : DependencyObject
        {
            return new DependencyProperty<TOwningType, TValueType>(name, defaultValue, onChange, validate);
        }
        public static DependencyProperty<TValueType> RegisterAttached<TOwningType, TValueType>(
                string name,
                TValueType defaultValue,
                PropertyChanged<DependencyObject, TValueType>? onChange = null,
                ValidateValue<DependencyObject, TValueType>? validate = null
            )
            where TOwningType : DependencyObject
        {
            return new DependencyProperty<TOwningType, TValueType>(name, defaultValue, onChange, validate);
        }

        public string Name { get; }
        public bool IsAttached { get; }
        public abstract Type PropertyType { get; }
        public abstract Type OwningType { get; }

        public Guid Guid { get; }

        public object? DefaultValue { get; }

        protected DependencyProperty(string name, bool isAttached, object? defaultValue)
        {
            Name = name;
            IsAttached = isAttached;
            Guid = Guid.NewGuid();
            DefaultValue = defaultValue;
        }

        public abstract bool ValidateValue(DependencyObject target, object? value);
        public abstract void ValueChanged(DependencyObject target, object? value);

        public abstract bool IsValidTarget(DependencyObject target);
    }

    public abstract class DependencyProperty<T> : DependencyProperty
    {
        protected DependencyProperty(string name, bool isAttached, T defaultValue) : base(name, isAttached, defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public new T DefaultValue { get; }

        public override Type PropertyType => typeof(T);

        public abstract bool ValidateValue(DependencyObject target, T value);

        public override sealed bool ValidateValue(DependencyObject target, object? value)
        {
            if (value is T tval)
                return ValidateValue(target, tval);

            if (default(T) is null && value is null)
                return ValidateValue(target, default!);

            return false;
        }

        public abstract void ValueChanged(DependencyObject target, T value);

        public override sealed void ValueChanged(DependencyObject target, object? value)
        {
            if (value is T tval)
                ValueChanged(target, tval);
            else if (default(T) is null && value is null)
                ValueChanged(target, default!);
            else
                throw new InvalidCastException("Value is not of the correct type");
        }
    }

    public class DependencyProperty<TOwner, T> : DependencyProperty<T>
        where TOwner : DependencyObject
    {
        private readonly PropertyChanged<TOwner, T>? onChanged = null;
        private readonly ValidateValue<TOwner, T>? validate = null;
        private readonly PropertyChanged<DependencyObject, T>? onChangedAtt = null;
        private readonly ValidateValue<DependencyObject, T>? validateAtt = null;

        static DependencyProperty()
        {
            if (typeof(TOwner) == typeof(DependencyObject))
                throw new InvalidOperationException("DependencyProperty's owner type cannot be DependencyObject");
        }

        internal DependencyProperty(
            string name,
            T defaultValue,
            PropertyChanged<TOwner, T>? onChanged,
            ValidateValue<TOwner, T>? validate
        ) : base(name, false, defaultValue)
        {
            this.onChanged = onChanged;
            this.validate = validate;
        }

        internal DependencyProperty(
            string name,
            T defaultValue,
            PropertyChanged<DependencyObject, T>? onChanged,
            ValidateValue<DependencyObject, T>? validate
        ) : base(name, true, defaultValue)
        {
            this.onChanged = onChanged;
            this.validate = validate;
        }

        public override Type OwningType => typeof(TOwner);

        public override bool ValidateValue(DependencyObject target, T value)
        {
            if (!IsValidTarget(target))
                return false;

            if (IsAttached)
                return validateAtt?.Invoke(target, value) ?? true;
            else
                return validate?.Invoke((TOwner)target, value) ?? true;
        }

        public override void ValueChanged(DependencyObject target, T value)
        {
            if (!IsValidTarget(target))
                throw new InvalidOperationException();

            if (IsAttached)
                onChangedAtt?.Invoke(target, value);
            else
                onChanged?.Invoke((TOwner)target, value);
        }

        public override bool IsValidTarget(DependencyObject target)
        {
            if (!IsAttached)
                return target is TOwner;

            return true;
        }
    }
}
