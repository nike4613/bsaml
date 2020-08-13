using System;
using System.Collections.Generic;
using System.Text;

namespace UnityPresentationFramework
{
    public abstract class DependencyObject
    {
        protected virtual DependencyObject? ParentObject => null;

        public object? GetValue(DependencyProperty prop)
        {
            if (!prop.IsValidTarget(this))
                throw new InvalidOperationException("This DependencyObject is not a valid target for the property");

            return GetInternal(prop);
        }

        public void SetValue(DependencyProperty prop, object? value)
        {
            if (!prop.ValidateValue(this, value))
                throw new InvalidOperationException("Invalid property value");

            SetInternal(prop, value);
        }
        public T GetValue<T>(DependencyProperty<T> prop)
        {
            if (!prop.IsValidTarget(this))
                throw new InvalidOperationException("This DependencyObject is not a valid target for the property");

            return (T)GetInternal(prop)!;
        }

        public void SetValue<T>(DependencyProperty<T> prop, T value)
        {
            if (!prop.ValidateValue(this, value))
                throw new InvalidOperationException("Invalid property value");

            SetInternal(prop, value, false);
            prop.ValueChanged(this, value);
        }

        private readonly Dictionary<Guid, object?> propertyValues = new Dictionary<Guid, object?>();

        private bool TryGetValue(DependencyProperty prop, out object? value)
            => propertyValues.TryGetValue(prop.Guid, out value);

        private object? GetInternal(DependencyProperty prop)
        {
            bool valueFound = false;
            object? value = null;
            var current = this;

            if (prop.IsInherited)
            {
                while (current != null && !(valueFound = current.TryGetValue(prop, out value)))
                {
                    current = current.ParentObject;
                }
            }
            else
            {
                valueFound = TryGetValue(prop, out value);
            }

            if (!valueFound)
            {
                value = prop.DefaultValue;
                propertyValues.Add(prop.Guid, value);
            }

            return value;
        }

        private void SetInternal(DependencyProperty prop, object? value, bool invokeChanged = true)
        {
            propertyValues[prop.Guid] = value;
            if (invokeChanged)
                prop.ValueChanged(this, value);
        }
    }
}
