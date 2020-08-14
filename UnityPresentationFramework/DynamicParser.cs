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
using UnityPresentationFramework.Parsing;

[assembly: XmlnsDefinition("upf", nameof(UnityPresentationFramework))]

namespace UnityPresentationFramework
{
    public class DynamicParser
    {
        private readonly IXamlReaderProvider ReaderProvider;
        //private readonly IBindingReflector Reflector;

        public DynamicParser(IXamlReaderProvider readerProvider/*, IBindingReflector reflector*/)
        {
            ReaderProvider = readerProvider;
            //Reflector = reflector;
        }

        public Element ParseXaml(string xaml)
        {
            using var sreader = new StringReader(xaml);
            return ParseXaml(sreader);
        }

        public Element ParseXaml(TextReader xamlReader)
        {
            using var xreader = ReaderProvider.FromTextReader(xamlReader);
            return ReadFromReader(xreader);
        }
        public Element ParseXaml(Stream xamlReader)
        {
            using var xreader = ReaderProvider.FromStream(xamlReader);
            return ReadFromReader(xreader);
        }

        private Element ReadFromReader(XamlReader xreader)
        {
            //var realReader = new UpfPostprocessingXamlReader(xreader, Reflector);
            using var objWriter = new XamlObjectWriter(xreader.SchemaContext, ReaderProvider.SettingsWithRoot(null));

            XamlServices.Transform(xreader, objWriter);

            var result = objWriter.Result as Element;
            if (result == null)
                throw new InvalidOperationException("Root element was not an Element");

            result.Finish();

            return result;
        }
    }
}
