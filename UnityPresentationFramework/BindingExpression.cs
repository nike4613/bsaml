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

        internal void Refresh(DependencyObject obj, DependencyProperty toProp, bool targetPropChanged)
        {
            if ((Binding.Direction & BindingDirection.OneWayToSource) != 0 && targetPropChanged)
            {
                var value = obj.GetValue(toProp);
                var context = obj.DataContext;
                if (context == null)
                    throw new NullReferenceException();
                Path.SetValue(context, value);
            }
            else if ((Binding.Direction & BindingDirection.OneWay) != 0)
            {
                var context = obj.DataContext;
                if (context == null)
                    throw new NullReferenceException();
                var value = Path.GetValue(context);
                obj.SetValue(toProp, value);
            }
        }
    }
}
