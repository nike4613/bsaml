using Knit.Utility;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Knit
{
    [DebuggerDisplay("Expression {Binding,nq} on {TargetObject} to {TargetProperty}")]
    public class BindingExpression
    {
        public Binding Binding { get; }

        public IBindingReflector Reflector { get; }
        public IDispatcher Dispatcher { get; }
        public IServiceProvider Services { get; }

        internal ILogger Logger { get; }

        public PropertyPath Path { get; }

        // This is needed so that I can have an absolute ordering of them
        internal readonly int _Id;

        private static int NextId = 0;
        public BindingExpression(Binding binding, IServiceProvider services)
        {
            _Id = Interlocked.Increment(ref NextId) - 1;

            Binding = binding;
            Services = services;
            Reflector = services.GetRequiredService<IBindingReflector>();
            Dispatcher = services.GetRequiredService<IDispatcher>();
            Logger = services.GetRequiredService<ILogger>().ForContext<BindingExpression>();
            Path = new PropertyPath(binding.Path.Split('.'), services);

            Logger.Verbose("Created BindingExpression for {@Binding} ({@Path})", Binding, Path);
        }

        public DependencyProperty? DependsOn 
            => ReferenceEquals(TargetProperty, DependencyObject.DataContextProperty)
            ? null : DependencyObject.DataContextProperty;

        public DependencyObject TargetObject => targetObj ?? throw new InvalidOperationException();
        public DependencyProperty TargetProperty => attachedProperty ?? throw new InvalidOperationException();

        private DependencyObject? targetObj;
        private DependencyProperty? attachedProperty;
        internal void AttachProperty(DependencyObject obj, DependencyProperty prop)
        {
            if (attachedProperty != null || targetObj != null)
                throw new InvalidOperationException();
            attachedProperty = prop;
            targetObj = obj;
        }
        private object? lastContext;

        private void Refresh(DependencyObject obj, bool targetPropChanged, Maybe<object?> knownValue)
        {
            if (attachedProperty == null || targetObj == null)
                throw new InvalidOperationException();
            if (!ReferenceEquals(obj, targetObj))
                throw new ArgumentException("A BindingExpression can be registered to only one DependencyObject and DepenencyProperty");

            var context = Binding.Source ?? obj.DataContext;
            if (context == null)
                throw new NullReferenceException();

            Logger.Verbose("Refreshing binding {@Binding} to {@TargetObject} on {@Property} (targetPropChanged: {PropChanged})", Binding, obj, attachedProperty, targetPropChanged);

            Logger.Verbose("Stack: {$Stack}", new EnhancedStackTrace(new StackTrace(true)));

            if ((Binding.Direction & BindingDirection.OneWayToSource) != 0 && targetPropChanged)
            {
                var value = knownValue || Maybe.Some(obj.GetValue(attachedProperty));
                Helpers.Assert(value.HasValue);
                Path.SetValue(context, value.Value);
            }
            else if ((Binding.Direction & BindingDirection.OneWay) != 0)
            {
                var value = knownValue || Maybe.Some(Path.GetValue(context));
                Helpers.Assert(value.HasValue);
                obj.SetValue(attachedProperty, value.Value);

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

        internal void QueueRefresh(DependencyObject obj, bool targetPropChanged)
        {
            Dispatcher.BeginInvoke(() => Refresh(obj, targetPropChanged, Maybe.None));
        }

        internal void RefreshSync(DependencyObject obj, bool targetPropChanged)
        {
            Dispatcher.Invoke(() => Refresh(obj, targetPropChanged, Maybe.None));
        }

        private void OnValueChanged(object source, object? value)
        {
            Dispatcher.BeginInvoke(() => Refresh(targetObj!, ReferenceEquals(source, targetObj!), Maybe.Some(value)));
        }
    }
}
