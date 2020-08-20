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
    internal class KnitXamlReaderProvider : IXamlReaderProvider
    {
        private readonly IServiceProvider Services;
        public KnitXamlReaderProvider(IServiceProvider services)
        {
            Services = services;
        }

        public XamlReader FromStream(Stream stream)
        {
            var baseReader = new XamlXmlReader(stream);
            return new KnitPostprocessingXamlReader(baseReader, Services);
        }

        public XamlReader FromTextReader(TextReader reader)
        {
            var baseReader = new XamlXmlReader(reader);
            return new KnitPostprocessingXamlReader(baseReader, Services);
        }

        public XamlObjectWriterSettings SettingsWithRoot(object? root)
            => new XamlObjectWriterSettings
            {
                RootObjectInstance = root
            };
    }
}
