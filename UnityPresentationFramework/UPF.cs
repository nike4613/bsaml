using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPresentationFramework.Parsing;

namespace UnityPresentationFramework
{
    public static class UPF
    {
        private static IServiceProvider CreateServices()
        {
            var collection = new ServiceCollection()
                .AddSingleton<IBindingReflector, SystemReflectionReflector>()
                .AddSingleton<IXamlReaderProvider, UpfXamlReaderProvider>()
                .AddSingleton<ILogger>(s => new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .Enrich.WithExceptionDetails()
                    .Enrich.WithDemystifiedStackTraces()
                    .Destructure.ByTransforming<PropertyPath>(p => new { p.Components })
                    .WriteTo.Console()
                    .CreateLogger())
                .AddSingleton<DynamicParser>();

            return collection.BuildServiceProvider();
        }

        internal static IServiceProvider Services { get; } = CreateServices();
        internal static ILogger Logger => Services.GetRequiredService<ILogger>();
        public static DynamicParser Parser => Services.GetRequiredService<DynamicParser>();
    }
}
