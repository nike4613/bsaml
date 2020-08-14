using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPresentationFramework
{
    public class BindingExpression
    {
        public Binding Binding { get; }

        public IBindingReflector Reflector { get; }

        public PropertyPath Path { get; }

        public BindingExpression(Binding binding, IBindingReflector reflector)
        {
            Binding = binding;
            Reflector = reflector;
            Path = new PropertyPath(binding.Path.Split('.'), reflector);
        }

        private DependencyObject? lastObj;
        private DependencyProperty? lastProp;
        private object? lastContext;

        internal void Refresh(DependencyObject obj, DependencyProperty toProp, bool targetPropChanged)
        {
            if ((lastObj != null && lastObj != obj)
             || (lastProp != null && lastProp != toProp))
                throw new ArgumentException("A BindingExpression can be registered to only one DependencyObject and DepenencyProperty");
            lastObj = obj;
            lastProp = toProp;

            var context = Binding.Source ?? obj.DataContext;
            if (context == null)
                throw new NullReferenceException();

            if ((Binding.Direction & BindingDirection.OneWayToSource) != 0 && targetPropChanged)
            {
                var value = obj.GetValue(toProp);
                Path.SetValue(context, value);
            }
            else if ((Binding.Direction & BindingDirection.OneWay) != 0)
            {
                var value = Path.GetValue(context);
                obj.SetValue(toProp, value);

                if (context != lastContext)
                {
                    if (lastContext != null)
                        Path.RemoveChangedHandler(lastContext, OnValueChanged);
                    Path.AddChangedHandler(context, OnValueChanged);
                    lastContext = context;
                }
            }

            lastContext = context;
        }

        private void OnValueChanged(object? value)
        {
            // TODO: somehow queue a refresh
        }
    }
}
