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
using UnityPresentationFramework.Parsing;

namespace UnityPresentationFramework
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

        public Binding(string path)
        {
            Path = path;
        }

        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            var targets = serviceProvider.GetService<IProvideValueTarget>();
            var schema = serviceProvider.GetService<IXamlSchemaContextProvider>().SchemaContext as UpfXamlSchemaContext;
            if (schema == null)
                throw new InvalidOperationException("Could not locate reflector");

            var targetObject = targets.TargetObject;

            if (!(targetObject is DependencyObject depObject))
                throw new InvalidOperationException("A binding cannot be added where there is no DependencyObject");

            if (!(targets.TargetProperty is DependencyProperty prop))
                throw new InvalidOperationException("Cannot bind to a property that is not a DependencyProperty");

            depObject.RegisterBinding(new BindingExpression(this, schema.Reflector), prop);

            return this;
        }

        internal static void HandleBindingSet(object? sender, XamlSetMarkupExtensionEventArgs args)
        {

        }
    }
}
