using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace UnityPresentationFramework.Parsing
{
    internal class UpfPostprocessingXamlReader : XamlReader
    {
        private readonly XamlReader reader;
        private readonly XamlType dependencyObjectType;

        public UpfPostprocessingXamlReader(XamlReader reader)
        {
            this.reader = reader;
            schemaContext = new UpfXamlSchemaContext(reader);
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

        private XamlMember TransformMember(XamlMember member)
        {
            var declType = member.DeclaringType;
            var name = member.Name;
            var attached = member.IsAttachable;

            var depProp = DependencyProperty.FromName(name, declType.UnderlyingType);
            if (depProp != null)
            {
                return new UpfXamlPropertyMember(depProp, new XamlType(depProp.OwningType, schemaContext), dependencyObjectType);
            }

            return member;
        }
    }
}
