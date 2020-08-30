using System;
using System.Collections.Generic;
using System.Text;

namespace Knit
{
    public abstract class DependencyObject
    {
        protected internal virtual DependencyObject? ParentObject => null;

        public static readonly DependencyProperty<object?> DataContextProperty =
            DependencyProperty.Register<DependencyObject, object?>(nameof(DataContext), null,
                onChange: (e, v) => 
                    {
                        e.refreshingDataContext = true;
                        e.RequestBindingRefresh(true);
                        e.refreshingDataContext = false;
                    },
                metadata: new DependencyMetadata { InheritsFromParent = true, ExcludedFromDataContextRefresh = true });

        public object? DataContext
        {
            get => GetValue(DataContextProperty);
            set => SetValue(DataContextProperty, value);
        }

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

        private class BindingExpressionComparer : IComparer<BindingExpression>
        {
            public static BindingExpressionComparer Default { get; } = new BindingExpressionComparer();

            public int Compare(BindingExpression x, BindingExpression y)
            {
                if (ReferenceEquals(x.DependsOn, y))
                    return 1;
                if (ReferenceEquals(y.DependsOn, x))
                    return -1;
                return x._Id - y._Id;
            }
        }

        private readonly SortedDictionary<BindingExpression, DependencyProperty> allBindings = new SortedDictionary<BindingExpression, DependencyProperty>(BindingExpressionComparer.Default);
        private readonly Dictionary<DependencyProperty, BindingExpression> inBindings = new Dictionary<DependencyProperty, BindingExpression>();
        private readonly Dictionary<DependencyProperty, BindingExpression> outBindings = new Dictionary<DependencyProperty, BindingExpression>();

        internal event Action<DependencyObject, DependencyProperty>? DependencyPropertyChanged;

        internal void RegisterBinding(BindingExpression binding, DependencyProperty prop)
        {
            binding.AttachProperty(this, prop);
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

        private bool refreshingDataContext = false;

        protected virtual void RequestBindingRefresh(bool includeOut)
        {
            // TODO: figure out a better way to handle dependencies

            if (!refreshingDataContext && inBindings.TryGetValue(DataContextProperty, out var dataCtxBinding))
            {
                // Triggering a refresh for DataContext will set it, which in turn re-triggers RequestBindingRefresh
                //   but with refreshingDataContext set. Therefore, since this is a synchronous update, all other
                //   bindings will have been updated when this returns.
                dataCtxBinding.RefreshSync(this, false);
            }
            else
            {
                foreach (var kvp in allBindings)
                {
                    if (kvp.Value == DataContextProperty) continue;
                    var isOut = (kvp.Key.Binding.Direction & BindingDirection.OneWayToSource) != 0;
                    if (isOut && !includeOut) continue;
                    if (!kvp.Value.Metadata.ExcludedFromDataContextRefresh || !refreshingDataContext)
                        kvp.Key.RefreshSync(this, isOut);
                }
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
                //propertyValues.Add(prop, value);
            }

            return value;
        }

        private void SetInternal(DependencyProperty prop, object? value, bool invokeChanged = true)
        {
            propertyValues[prop] = value;
            if (outBindings.TryGetValue(prop, out var binding))
            {
                binding.QueueRefresh(this, true); // this should then propagate it up if needed
            }
            DependencyPropertyChanged?.Invoke(this, prop);
            if (invokeChanged)
                prop.ValueChanged(this, value);
        }
    }
}
