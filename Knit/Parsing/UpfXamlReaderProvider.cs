using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace Knit.Parsing
{
    internal class UpfXamlReaderProvider : IXamlReaderProvider
    {
        private readonly IServiceProvider Services;
        public UpfXamlReaderProvider(IServiceProvider services)
        {
            Services = services;
        }

        public XamlReader FromStream(Stream stream)
        {
            var baseReader = new XamlXmlReader(stream);
            return new UpfPostprocessingXamlReader(baseReader, Services);
        }

        public XamlReader FromTextReader(TextReader reader)
        {
            var baseReader = new XamlXmlReader(reader);
            return new UpfPostprocessingXamlReader(baseReader, Services);
        }

        public XamlObjectWriterSettings SettingsWithRoot(object? root)
            => new XamlObjectWriterSettings
            {
                RootObjectInstance = root
            };
    }
}
