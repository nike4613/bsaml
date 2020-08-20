using Knit;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSAML
{
    public static class BSAMLCore
    {
        private static IServiceProvider CreateServices()
        {
            var collection = new ServiceCollection()
                .AddKnitServices(withDefaultReflector: true)
                .AddSingleton<IDispatcher, ThreadDispatcher>()
                .AddSingleton<ILogger>(s => new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .Enrich.WithExceptionDetails()
                    .Enrich.WithDemystifiedStackTraces()
                    .Destructure.KnitTypes()
                    .WriteTo.Console()
                    .CreateLogger())
                .AddSingleton<DynamicParser>();
            var provider = collection.BuildServiceProvider();
            if (!KnitServices.ValidateServices(provider))
                throw new InvalidOperationException("Knit services not prepared correctly");
            return provider;
        }

        public static void Close()
        {
            using var disp = Services.GetService<IDispatcher>() as IDisposable;
        }

        internal static IServiceProvider Services { get; } = CreateServices();
        internal static ILogger Logger => Services.GetRequiredService<ILogger>();
        internal static IDispatcher Dispatcher => Services.GetRequiredService<IDispatcher>();

        public static DynamicParser Parser => Services.GetRequiredService<DynamicParser>();
    }
}
