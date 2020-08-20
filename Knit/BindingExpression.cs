using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knit
{
    public class BindingExpression
    {
        public Binding Binding { get; }

        public IBindingReflector Reflector { get; }
        public IDispatcher Dispatcher { get; }
        public IServiceProvider Services { get; }

        public PropertyPath Path { get; }

        public BindingExpression(Binding binding, IServiceProvider services)
        {
            Binding = binding;
            Services = services;
            Reflector = services.GetRequiredService<IBindingReflector>();
            Dispatcher = services.GetRequiredService<IDispatcher>();
            Path = new PropertyPath(binding.Path.Split('.'), services);
        }


        private DependencyObject? lastObj;
        private DependencyProperty? lastProp;
        private object? lastContext;

        private void Refresh(DependencyObject obj, DependencyProperty toProp, bool targetPropChanged)
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
                    lastContext = context;
                }

                Path.AddChangedHandler(context, OnValueChanged);
            }

            lastContext = context;
        }

        internal void QueueRefresh(DependencyObject obj, DependencyProperty toProp, bool targetPropChanged)
        {
            Dispatcher.BeginInvoke(() => Refresh(obj, toProp, targetPropChanged));
        }

        private void OnValueChanged(object? value)
        {
            // TODO: somehow queue a refresh
            QueueRefresh(lastObj!, lastProp!, false);
        }
    }
}
