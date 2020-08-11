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

            Type propType;
            var prop = targets.TargetProperty;
            if (prop is PropertyInfo propInfo)
            {
                propType = propInfo.PropertyType;
            }
            else if (prop is FieldInfo field)
            {
                propType = field.FieldType;
            }
            else if (prop is DependencyProperty depProp)
            {
                propType = depProp.PropertyType;
            }
            else
            {
                throw new InvalidOperationException("Unknown type of property");
            }

            object result;
            if (propType.IsConstructedGenericType && propType.GetGenericTypeDefinition() == typeof(Bindable<>))
            {
                var type = propType.GetGenericArguments().First();
                var bindingType = typeof(Binding<>).MakeGenericType(type);
                var ctor = bindingType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(Binding) }, Array.Empty<ParameterModifier>());
                result = ctor.Invoke(new[] { this });
            }
            else if (propType == typeof(object))
            {
                result = new Binding<object>(this);
            }
            else
            {
                throw new InvalidOperationException("Cannot bind to a non-Bindable property");
            }

            return result;
        }

        internal static void HandleBindingSet(object? sender, XamlSetMarkupExtensionEventArgs args)
        {

        }
    }

    public class Binding<T> : Bindable<T>, IBinding
    {
        public override T Value => throw new NotImplementedException();

        public object? Source { get; }

        public string Path { get; }

        internal Binding(Binding bind)
        {
            Source = bind.Source;
            Path = bind.Path;
        }

        public static implicit operator Binding<T>(Binding b)
            => new Binding<T>(b);
    }
}
