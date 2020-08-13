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

        internal void ResetAllValues()
        {
            allBindings.Clear();
            inBindings.Clear();
            outBindings.Clear();
            propertyValues.Clear();
        }

        private readonly Dictionary<BindingExpression, DependencyProperty> allBindings = new Dictionary<BindingExpression, DependencyProperty>();
        private readonly Dictionary<DependencyProperty, BindingExpression> inBindings = new Dictionary<DependencyProperty, BindingExpression>();
        private readonly Dictionary<DependencyProperty, BindingExpression> outBindings = new Dictionary<DependencyProperty, BindingExpression>();

        internal void RegisterBinding(BindingExpression binding, DependencyProperty prop)
        {
            if ((binding.Binding.Direction & BindingDirection.OneWay) != 0)
            {
                inBindings.Add(prop, binding);
            }
            if ((binding.Binding.Direction & BindingDirection.OneWayToSource) != 0)
            {
                outBindings.Add(prop, binding);
            }
            allBindings.Add(binding, prop);
        }

        protected virtual void RequestBindingRefresh()
        {
            // TODO: make this queue to some dispatcher
            foreach (var kvp in allBindings)
            {
                kvp.Key.Refresh(this, kvp.Value);
            }
        }

        private readonly Dictionary<DependencyProperty, object?> propertyValues = new Dictionary<DependencyProperty, object?>();

        private bool TryGetValue(DependencyProperty prop, out object? value)
            => propertyValues.TryGetValue(prop, out value);

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
                propertyValues.Add(prop, value);
            }

            return value;
        }

        private void SetInternal(DependencyProperty prop, object? value, bool invokeChanged = true)
        {
            propertyValues[prop] = value;
            if (outBindings.TryGetValue(prop, out var binding))
            {
                // TODO: implement this as a potentially queued item to be executed soon
                binding.Refresh(this, prop); // this should then propagate it up if needed
            }
            if (invokeChanged)
                prop.ValueChanged(this, value);
        }
    }
}
