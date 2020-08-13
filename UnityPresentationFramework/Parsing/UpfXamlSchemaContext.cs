﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace UnityPresentationFramework.Parsing
{
    internal class UpfXamlSchemaContext : XamlSchemaContext
    {
        private readonly XamlReader underlyingReader;
        public UpfXamlSchemaContext(XamlReader under)
            : base(under.SchemaContext.ReferenceAssemblies,
                  new XamlSchemaContextSettings
                  {
                      FullyQualifyAssemblyNamesInClrNamespaces = under.SchemaContext.FullyQualifyAssemblyNamesInClrNamespaces,
                      SupportMarkupExtensionsWithDuplicateArity = under.SchemaContext.SupportMarkupExtensionsWithDuplicateArity
                  })
        {
            underlyingReader = under;
        }

        public XamlSchemaContext UnderlyingContext => underlyingReader.SchemaContext;
    }
}
