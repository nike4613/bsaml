using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Knit.Parsing;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Knit
{
    public static class KnitServices
    {
        public static IServiceCollection AddKnitServices(this IServiceCollection services, bool withDefaultReflector = true)
        {
            services.AddScoped<IXamlReaderProvider, KnitXamlReaderProvider>();
            if (withDefaultReflector)
                services.AddScoped<IBindingReflector, SystemReflectionReflector>();
            return services;
        }

        public static LoggerConfiguration KnitTypes(this LoggerDestructuringConfiguration config)
            => config.ByTransforming<PropertyPath>(p => new { p.Components })
            .Destructure.With(new KnitDestructuringPolicy());

        public static bool ValidateServices(IServiceProvider services)
            => services.GetService<ILogger>() != null
            && services.GetService<IBindingReflector>() != null
            && services.GetService<IDispatcher>() != null;

        private class KnitDestructuringPolicy : IDestructuringPolicy
        {
            public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [MaybeNullWhen(false)] out LogEventPropertyValue result)
            {
                if (value is DependencyProperty prop)
                {
                    result = propertyValueFactory.CreatePropertyValue(new { prop.Name, Type = prop.PropertyType, prop.OwningType }, true);
                    return true;
                }

                result = null;
                return false;
            }
        }
    }
}
