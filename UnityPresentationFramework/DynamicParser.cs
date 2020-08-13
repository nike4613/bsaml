using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xaml;
using System.Xml;

[assembly: XmlnsDefinition("upf", nameof(UnityPresentationFramework))]

namespace UnityPresentationFramework
{
    public static class DynamicParser
    {
        public static Element ParseXaml(string xaml)
        {
            using var sreader = new StringReader(xaml);
            return ParseXaml(sreader);
        }

        public static Element ParseXaml(TextReader xamlReader)
        {
            using var xreader = new XamlXmlReader(xamlReader);
            return ReadFromReader(xreader);
        }
        public static Element ParseXaml(Stream xamlReader)
        {
            using var xreader = new XamlXmlReader(xamlReader);
            return ReadFromReader(xreader);
        }

        private static Element ReadFromReader(XamlXmlReader xreader)
        {
            using var objWriter = new XamlObjectWriter(xreader.SchemaContext, new XamlObjectWriterSettings { });

            XamlServices.Transform(xreader, objWriter);

            var result = objWriter.Result as Element;
            if (result == null)
                throw new InvalidOperationException("Root element was not an Element");

            result.Finish();

            return result;
        }
    }
}
