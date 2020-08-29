using IPA;
using IPA.Loader;
using IPA.Utilities.Async;
using Knit;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;

namespace BSAML
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin : ILogEventSink
    {
        private readonly IPALogger logger;

        private readonly IServiceProvider services;

        private readonly UnityMainThreadTaskScheduler scheduler;

        [Init]
        public Plugin(IPALogger logger)
        {
            this.logger = logger;
            scheduler = new UnityMainThreadTaskScheduler();
            services = PrepareServices();

            PluginInitInjector.AddInjector(typeof(DynamicParser), (prev, param, meta) =>
            {
                if (prev != null) return prev;
                var parser = new DynamicParser(
                    services.GetRequiredService<IXamlReaderProvider>(),
                    services.GetRequiredService<ILogger>().ForContext("ForPlugin", meta, false),
                    services
                );
                return parser;
            });
        }

        private IServiceProvider PrepareServices()
        {
            var collection = new ServiceCollection()
                .AddSingleton(this)
                .AddKnitServices(withDefaultReflector: true)
                .AddSingleton<TaskScheduler, UnityMainThreadTaskScheduler>(s => s.GetRequiredService<Plugin>().scheduler)
                .AddSingleton<TaskFactory>()
                .AddSingleton<IDispatcher, TaskDispatcher>()
                .AddSingleton<ILogger>(s => new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .Enrich.WithExceptionDetails()
                    .Enrich.WithDemystifiedStackTraces()
                    .Destructure.KnitTypes()
                    .Destructure.AsScalar<PluginMetadata>()
                    .WriteTo.Sink(s.GetRequiredService<Plugin>())
                    .CreateLogger())
                .AddTransient<DynamicParser>();

            var provider = collection.BuildServiceProvider();
            if (!KnitServices.ValidateServices(provider))
                throw new InvalidOperationException("Knit services not prepared correctly");
            return provider;
        }

        #region ILogEventSink implementation
        private static T Do<T>(Action thing, T val)
        {
            thing();
            return val;
        }

        void ILogEventSink.Emit(LogEvent logEvent)
        {
            var level = logEvent.Level switch
            {
                LogEventLevel.Verbose => IPALogger.Level.Trace,
                LogEventLevel.Debug => IPALogger.Level.Debug,
                LogEventLevel.Information => IPALogger.Level.Info,
                LogEventLevel.Warning => IPALogger.Level.Warning,
                LogEventLevel.Error => IPALogger.Level.Error,
                LogEventLevel.Fatal => IPALogger.Level.Critical,
                _ => Do(() => logger.Warn($"Invalid Serilog level {logEvent.Level}"), IPALogger.Level.Info),
            };
            string prefix = "";

            if (logEvent.Properties.TryGetValue("ForPlugin", out var value)
                 && value is ScalarValue scalar
                 && scalar.Value is PluginMetadata meta)
            {
                prefix += $"[{meta.Name}] ";
            }

            if (logEvent.Properties.TryGetValue("SourceContext", out value))
            {
                prefix += $"{{{value}}}: ";
            }

            logger.Log(level, prefix + logEvent.RenderMessage());
            if (logEvent.Exception != null)
                logger.Log(level, logEvent.Exception);
        }
        #endregion
    }
}
