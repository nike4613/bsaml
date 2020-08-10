using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Markup;
using System.Xaml;

namespace UnityPresentationFramework
{
    [XamlSetMarkupExtension(nameof(HandleBindingSet))]
    public class Binding : MarkupExtension
    {
        public object? Source { get; set; } = null;

        public string Path { get; set; }

        public Binding(string path)
        {
            Path = path;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var targets = serviceProvider.GetService<IProvideValueTarget>();
            var schema = serviceProvider.GetService<IXamlSchemaContextProvider>().SchemaContext;

            var targetObject = targets.TargetObject;

            if (!(targetObject is DependencyObject depObject))
                throw new InvalidOperationException("A binding cannot be added where there is no DependencyObject");

            if (depObject is Element element)
            {
                Source ??= element.DataContext;
            }

            // TODO: how do I make this work sanely?

            /*if (Source is null)
                throw new InvalidOperationException("Cannot bind to a property on a null source");*/

            return null!;
        }

        internal static void HandleBindingSet(object? sender, XamlSetMarkupExtensionEventArgs args)
        {

        }
    }
}
