using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml.Schema;

namespace Knit.Parsing
{
    internal class KnitDependencyPropMemberInvoker : XamlMemberInvoker
    {
        private readonly DependencyProperty prop;
        internal KnitDependencyPropMemberInvoker(DependencyProperty prop)
        {
            this.prop = prop;
        }

        public override object? GetValue(object instance)
        {
            var dep = Validate(instance);
            return dep.GetValue(prop);
        }

        public override void SetValue(object instance, object? value)
        {
            var dep = Validate(instance);
            if (value is Binding)
            {
                // if our value is a binding, then we don't actually want to set it
                // the binding will have been bound, and will be updated later
                return;
            }
            else
            {
                dep.SetValue(prop, value);
            }
        }

        private static DependencyObject Validate(object instance)
        {
            if (!(instance is DependencyObject depObj))
                throw new ArgumentException("Instance must be a DependencyObject", nameof(instance));
            return depObj;
        }
    }
}
