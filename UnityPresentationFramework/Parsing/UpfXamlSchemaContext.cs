using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace Knit.Parsing
{
    internal class UpfXamlSchemaContext : XamlSchemaContext
    {
        private readonly XamlReader underlyingReader;
        public UpfXamlSchemaContext(XamlReader under, IServiceProvider services)
            : base(under.SchemaContext.ReferenceAssemblies,
                  new XamlSchemaContextSettings
                  {
                      FullyQualifyAssemblyNamesInClrNamespaces = under.SchemaContext.FullyQualifyAssemblyNamesInClrNamespaces,
                      SupportMarkupExtensionsWithDuplicateArity = under.SchemaContext.SupportMarkupExtensionsWithDuplicateArity
                  })
        {
            underlyingReader = under;
            Services = services;
            Reflector = services.GetService<IBindingReflector>();
        }

        public XamlSchemaContext UnderlyingContext => underlyingReader.SchemaContext;

        public IBindingReflector Reflector { get; }

        public IServiceProvider Services { get; }
    }
}
