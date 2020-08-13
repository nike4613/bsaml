using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Markup;
using System.Xaml;
using System.Reflection;

namespace UnityPresentationFramework
{
    public interface IBinding
    {
        public object? Source { get; }

        // TODO: figure out a better abstraction for a path like this
        public string Path { get; }
    }

    [XamlSetMarkupExtension(nameof(HandleBindingSet))]
    public class Binding : MarkupExtension
    {
        public string Path { get; set; }

        public Binding(string path)
        {
            Path = path;
        }

        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            var targets = serviceProvider.GetService<IProvideValueTarget>();
            var schema = serviceProvider.GetService<IXamlSchemaContextProvider>().SchemaContext;

            var targetObject = targets.TargetObject;

            if (!(targetObject is DependencyObject depObject))
                throw new InvalidOperationException("A binding cannot be added where there is no DependencyObject");

            if (!(targets.TargetProperty is DependencyProperty prop))
                throw new InvalidOperationException("Cannot bind to a property that is not a DependencyProperty");

            return this;
        }

        internal static void HandleBindingSet(object? sender, XamlSetMarkupExtensionEventArgs args)
        {

        }
    }
}
