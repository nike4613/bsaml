using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;

namespace UnityPresentationFramework.Parsing
{
    internal class UpfXamlPropertyMember : XamlMember, IProvideValueTarget
    {
        private readonly DependencyProperty prop;
        private readonly XamlType owner;
        private readonly XamlType depObjType;
        private readonly UpfDependencyPropMemberInvoker invoker;

        public UpfXamlPropertyMember(DependencyProperty prop, XamlType owner, XamlType depObjType) 
            : base(prop.Name, owner, prop.IsAttached)
        {
            this.prop = prop;
            this.owner = owner;
            this.depObjType = depObjType;
            invoker = new UpfDependencyPropMemberInvoker(prop);
        }

        protected override XamlMemberInvoker LookupInvoker() => invoker;

        protected override bool LookupIsEvent() => false;
        protected override bool LookupIsReadOnly() => false;
        protected override bool LookupIsReadPublic() => true;
        protected override bool LookupIsWriteOnly() => false;
        protected override bool LookupIsWritePublic() => true;
        protected override bool LookupIsUnknown() => false;
        protected override XamlType LookupTargetType()
        {
            if (prop.IsAttached)
                return depObjType;
            else
                return owner;
        }
        protected override XamlType LookupType()
            => new XamlType(prop.PropertyType, owner.SchemaContext);

        object? IProvideValueTarget.TargetObject => null;

        object IProvideValueTarget.TargetProperty => prop;
    }
}
