using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Markup;
using System.Xaml;
using System.Reflection;
using System.Diagnostics;
using Knit.Parsing;
using Serilog;

[assembly: XmlnsDefinition("knit", nameof(Knit))]

namespace Knit
{
    public enum BindingDirection
    {
        OneWay = 0x1,
        TwoWay = OneWay | OneWayToSource,
        OneWayToSource = 0x2,
    }

    [XamlSetMarkupExtension(nameof(HandleBindingSet))]
    [DebuggerDisplay("\\{Binding {Path,nq}, Direction={Direction}\\}")]
    public class Binding : MarkupExtension
    {
        public BindingDirection Direction { get; set; } = BindingDirection.OneWay;

        public string Path { get; }
        
        public object? Source { get; set; }

        public Binding(string path)
        {
            Path = path;
        }

        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            var targets = serviceProvider.GetService<IProvideValueTarget>();
            var schema = serviceProvider.GetService<IXamlSchemaContextProvider>().SchemaContext as KnitXamlSchemaContext;
            if (schema == null)
                throw new InvalidOperationException("Could not locate reflector");

            var logger = schema.Services.GetRequiredService<ILogger>().ForContext<Binding>();
            logger.Verbose("Connecting binding {@Binding}", this);

            var targetObject = targets.TargetObject;

            if (!(targetObject is DependencyObject depObject))
                throw new InvalidOperationException("A binding cannot be added where there is no DependencyObject");

            if (!(targets.TargetProperty is DependencyProperty prop))
                throw new InvalidOperationException("Cannot bind to a property that is not a DependencyProperty");

            logger.Verbose("Target object and property found: obj = {@Object}, prop = {@Property}", depObject, prop);
            logger.Verbose("Registering to dependency object");

            depObject.RegisterBinding(new BindingExpression(this, schema.Services), prop);

            return this;
        }

        internal static void HandleBindingSet(object? sender, XamlSetMarkupExtensionEventArgs args)
        {

        }
    }
}
