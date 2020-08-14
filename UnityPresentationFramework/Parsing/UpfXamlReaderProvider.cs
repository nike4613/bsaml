using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace UnityPresentationFramework.Parsing
{
    internal class UpfXamlReaderProvider : IXamlReaderProvider
    {
        private readonly IBindingReflector Reflector;
        public UpfXamlReaderProvider(IBindingReflector reflector)
        {
            Reflector = reflector;
        }

        public XamlReader FromStream(Stream stream)
        {
            var baseReader = new XamlXmlReader(stream);
            return new UpfPostprocessingXamlReader(baseReader, Reflector);
        }

        public XamlReader FromTextReader(TextReader reader)
        {
            var baseReader = new XamlXmlReader(reader);
            return new UpfPostprocessingXamlReader(baseReader, Reflector);
        }

        public XamlObjectWriterSettings SettingsWithRoot(object? root)
            => new XamlObjectWriterSettings
            {
                RootObjectInstance = root
            };
    }
}
