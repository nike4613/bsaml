﻿using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xaml;
using System.Xml;
using Knit.Parsing;

[assembly: XmlnsDefinition("upf", nameof(Knit))]

namespace Knit
{
    public class DynamicParser
    {
        private readonly IXamlReaderProvider ReaderProvider;
        private readonly ILogger Logger;
        private readonly IServiceProvider Services;

        public DynamicParser(IXamlReaderProvider readerProvider, ILogger logger, IServiceProvider services)
        {
            ReaderProvider = readerProvider;
            Services = services;
            Logger = logger;
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

            var dispatcher = Services.GetRequiredService<IDispatcher>();

            var waitHandle = new ManualResetEventSlim(false);
            try
            {
                dispatcher.BeginInvoke(() => waitHandle.Wait()); // ensure no queued actions happen until we're done

                XamlServices.Transform(xreader, objWriter);

                var result = objWriter.Result as Element;
                if (result == null)
                    throw new InvalidOperationException("Root element was not an Element");

                result.Attach();

                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "An error ocurred while parsing and constructing XAML");

                throw new XamlParseException(e);
            }
            finally
            {
                waitHandle.Set();
                dispatcher.Invoke(() => { }); // wait for the queue to finish
            }
        }
    }

    public class XamlParseException : Exception
    {
        public XamlParseException(Exception e) : base("Error while parsing and constructing XAML structure", e)
        {
        }
    }
}
