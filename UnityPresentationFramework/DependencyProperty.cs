﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace UnityPresentationFramework
{
    public delegate bool ValidateValue<in TOwner, in T>(TOwner owner, T value) where TOwner : DependencyObject;
    public delegate void PropertyChanged<in TOwner, in T>(TOwner owner, T value) where TOwner : DependencyObject;

    [DebuggerDisplay("{flags}")]
    public struct DependencyMetadata
    {
        [Flags]
        private enum Flags : byte
        {
            None = 0,
            InheritsParent = 0x01,

            _All = 0xFF,
        }

        private Flags flags;
        public bool InheritsFromParent
        {
            get => 0 != (flags & Flags.InheritsParent);
            set => flags = (flags & ~Flags.InheritsParent) | (value ? Flags.InheritsParent : 0);
        }
    }

    [DebuggerDisplay("{PropertyType.FullName,nq} {Name,nq} \\{IsAttached={IsAttached}, Owner={OwningType.FullName,nq}\\}")]
    public abstract class DependencyProperty
    {
        private static readonly object syncObject = new object();
        private static readonly Dictionary<(Type Owner, string Name), DependencyProperty> properties = new Dictionary<(Type, string), DependencyProperty>();

        public static DependencyProperty<TValueType> Register<TOwningType, TValueType>(
                string name,
                TValueType defaultValue,
                PropertyChanged<TOwningType, TValueType>? onChange = null,
                ValidateValue<TOwningType, TValueType>? validate = null,
                DependencyMetadata metadata = default
            )
            where TOwningType : DependencyObject
        {
            var key = (type: typeof(TOwningType), name);

            lock (syncObject)
            {
                if (properties.ContainsKey(key))
                    throw new ArgumentException($"Property '{name}' already registered on {key.type.Name}");

                var prop = new DependencyProperty<TOwningType, TValueType>(name, defaultValue, onChange, validate, metadata);

                properties.Add(key, prop);

                return prop;
            }
        }
        public static DependencyProperty<TValueType> RegisterAttached<TOwningType, TValueType>(
                string name,
                TValueType defaultValue,
                PropertyChanged<DependencyObject, TValueType>? onChange = null,
                ValidateValue<DependencyObject, TValueType>? validate = null,
                DependencyMetadata metadata = default
            )
            where TOwningType : DependencyObject
        {
            var key = (type: typeof(TOwningType), name);

            lock (syncObject)
            {
                if (properties.ContainsKey(key))
                    throw new ArgumentException($"Property '{name}' already registered on {key.type.Name}");

                var prop = new DependencyProperty<TOwningType, TValueType>(name, defaultValue, onChange, validate, metadata);

                properties.Add(key, prop);

                return prop;
            }
        }

        internal static DependencyProperty? FromName(string name, Type owner)
        {
            Type? ownerType = owner;
            DependencyProperty? prop = null;
            while (prop == null)
            {
                if (ownerType is null) break;

                RuntimeHelpers.RunClassConstructor(ownerType.TypeHandle);

                lock (syncObject)
                {
                    if (!properties.TryGetValue((ownerType, name), out prop))
                        prop = null;
                }

                ownerType = ownerType.BaseType;
            }
            return prop;
        }

        public string Name { get; }
        public bool IsAttached { get; }
        public abstract Type PropertyType { get; }
        public abstract Type OwningType { get; }

        public object? DefaultValue { get; }

        public bool IsInherited => Metadata.InheritsFromParent;

        protected DependencyMetadata Metadata { get; }

        protected DependencyProperty(string name, bool isAttached, object? defaultValue, DependencyMetadata metadata)
        {
            Name = name;
            IsAttached = isAttached;
            DefaultValue = defaultValue;
            Metadata = metadata;
        }

        public abstract bool ValidateValue(DependencyObject target, object? value);
        public abstract void ValueChanged(DependencyObject target, object? value);

        public abstract bool IsValidTarget(DependencyObject target);
    }

    [DebuggerDisplay("{PropertyType.FullName,nq} {Name,nq} \\{IsAttached={IsAttached}, Owner={OwningType.FullName,nq}\\}")]
    public abstract class DependencyProperty<T> : DependencyProperty
    {
        protected DependencyProperty(string name, bool isAttached, T defaultValue, DependencyMetadata metadata) 
            : base(name, isAttached, defaultValue, metadata)
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

    [DebuggerDisplay("{PropertyType.FullName,nq} {Name,nq} \\{IsAttached={IsAttached}, Owner={OwningType.FullName,nq}\\}")]
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
            ValidateValue<TOwner, T>? validate,
            DependencyMetadata metadata
        ) : base(name, false, defaultValue, metadata)
        {
            this.onChanged = onChanged;
            this.validate = validate;
        }

        internal DependencyProperty(
            string name,
            T defaultValue,
            PropertyChanged<DependencyObject, T>? onChanged,
            ValidateValue<DependencyObject, T>? validate,
            DependencyMetadata metadata
        ) : base(name, true, defaultValue, metadata)
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
