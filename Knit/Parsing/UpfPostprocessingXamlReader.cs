using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace Knit.Parsing
{
    internal class UpfPostprocessingXamlReader : XamlReader
    {
        private readonly XamlReader reader;
        private readonly XamlType dependencyObjectType;

        public UpfPostprocessingXamlReader(XamlReader reader, IServiceProvider services)
        {
            this.reader = reader;
            schemaContext = new UpfXamlSchemaContext(reader, services);
            dependencyObjectType = new XamlType(typeof(DependencyObject), schemaContext);
        }

        private XamlNodeType nodeType = default;
        public override XamlNodeType NodeType => nodeType;

        private bool isEof = false;
        public override bool IsEof => isEof;

        private NamespaceDeclaration? namespaceDecl = null;
        public override NamespaceDeclaration? Namespace => namespaceDecl;

        private XamlType? type = null;
        public override XamlType? Type => type;

        private object? value = null;
        public override object? Value => value;

        private XamlMember? member = null;
        public override XamlMember? Member => member;

        private readonly UpfXamlSchemaContext schemaContext;
        public override XamlSchemaContext SchemaContext => schemaContext;

        public override bool Read()
        {
            var result = reader.Read();

            if (result)
            {
                nodeType = reader.NodeType;
                isEof = reader.IsEof;
                namespaceDecl = reader.Namespace;
                type = reader.Type;
                value = reader.Value;
                member = reader.Member;

                if (member != null && member.GetType() == typeof(XamlMember))
                    member = TransformMember(member);
            }

            return result;
        }

        private readonly Dictionary<Type, XamlType> ownerTypeCache = new Dictionary<Type, XamlType>();
        private readonly ConditionalWeakTable<DependencyProperty, UpfXamlPropertyMember> propCache = new ConditionalWeakTable<DependencyProperty, UpfXamlPropertyMember>();

        private XamlMember TransformMember(XamlMember member)
        {
            var declType = member.DeclaringType;
            var name = member.Name;
            var attached = member.IsAttachable;

            var depProp = DependencyProperty.FromName(name, declType.UnderlyingType);
            if (depProp != null)
            {
                if (!propCache.TryGetValue(depProp, out var uMember))
                {
                    if (!ownerTypeCache.TryGetValue(depProp.OwningType, out var xamlType))
                        ownerTypeCache.Add(depProp.OwningType, xamlType = new XamlType(depProp.OwningType, schemaContext));
                    propCache.Add(depProp, uMember = new UpfXamlPropertyMember(depProp, xamlType, dependencyObjectType));
                }
                return uMember;
            }

            return member;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (reader is IDisposable disp)
            {
                disp.Dispose();
            }
        }
    }
}
