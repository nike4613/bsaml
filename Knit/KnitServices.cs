using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Knit.Parsing;
using Serilog.Configuration;

namespace Knit
{
    public static class KnitServices
    {
        public static IServiceCollection AddKnitServices(this IServiceCollection services, bool withDefaultReflector = true)
        {
            services.AddSingleton<IXamlReaderProvider, KnitXamlReaderProvider>();
            if (withDefaultReflector)
                services.AddSingleton<IBindingReflector, SystemReflectionReflector>();
            return services;
        }

        public static LoggerConfiguration KnitTypes(this LoggerDestructuringConfiguration config)
            => config.ByTransforming<PropertyPath>(p => new { p.Components });

        public static bool ValidateServices(IServiceProvider services)
            => services.GetService<ILogger>() != null
            && services.GetService<IBindingReflector>() != null
            && services.GetService<IDispatcher>() != null;
    }
}
