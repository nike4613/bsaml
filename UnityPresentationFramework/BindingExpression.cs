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

        public BindingExpression(Binding binding, IBindingReflector reflector)
        {
            Binding = binding;
            Reflector = reflector;
        }

        internal void Refresh(DependencyObject obj, DependencyProperty toProp)
        {

        }
    }
}
