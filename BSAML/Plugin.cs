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
using UnityEngine;
using UnityObject = UnityEngine.Object;
using IPALogger = IPA.Logging.Logger;
using ILogger = Serilog.ILogger;
using System.Diagnostics.CodeAnalysis;

namespace BSAML
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin : ILogEventSink, IDestructuringPolicy
    {
        private readonly IPALogger ipaLogger;
        private readonly PluginMetadata ownMeta;

        private readonly IServiceProvider services;
        private readonly ILogger logger;

        private readonly UnityMainThreadTaskScheduler scheduler;

        [Init]
        public Plugin(IPALogger logger, PluginMetadata meta)
        {
            ipaLogger = logger;
            ownMeta = meta;
            scheduler = new UnityMainThreadTaskScheduler();
            services = PrepareServices();
            this.logger = services.GetRequiredService<ILogger>().ForContext<Plugin>();

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

        private class CoroHost : MonoBehaviour { }
        private GameObject? schedulerHostObject;
        private CoroHost? schedulerHost;

        [OnEnable]
        public void OnEnable()
        {
            logger.Debug("Creating scheduler host object");

            schedulerHostObject = new GameObject(ownMeta.Name + " Dispatcher Scheduler Host");
            UnityObject.DontDestroyOnLoad(schedulerHostObject);
            schedulerHost = schedulerHostObject.AddComponent<CoroHost>();
            UnityObject.DontDestroyOnLoad(schedulerHost);

            // start the scheduler
            schedulerHost.StartCoroutine(scheduler.Coroutine());

            logger.Debug("Plugin enabled");
        }

        [OnDisable]
        public async Task OnDisable()
        {
            logger.Debug("Destroying scheduler host object");

            // cancel and wait for it to exit
            scheduler.Cancel();
            while (scheduler.IsRunning)
                await Task.Yield();

            UnityObject.Destroy(schedulerHost);
            UnityObject.Destroy(schedulerHostObject);

            logger.Debug("Plugin disabled");
        }

        private IServiceProvider PrepareServices()
        {
            var collection = new ServiceCollection()
                .AddSingleton(this)
                .AddKnitServices(withDefaultReflector: false)
                .AddScoped<IBindingReflector, BSIPAAccessorReflector>()
                .AddSingleton<TaskScheduler, UnityMainThreadTaskScheduler>(s => s.GetRequiredService<Plugin>().scheduler)
                .AddSingleton<TaskFactory>()
                .AddSingleton<IDispatcher, TaskDispatcher>()
                .AddSingleton<ILogger>(s => new LoggerConfiguration()
#if DEBUG
                    .MinimumLevel.Verbose()
#else
                    .MinimumLevel.Debug()
#endif
                    .Enrich.WithExceptionDetails()
                    .Enrich.WithDemystifiedStackTraces()
                    .Destructure.KnitTypes()
                    .Destructure.AsScalar<PluginMetadata>()
                    .Destructure.With(s.GetRequiredService<Plugin>())
                    .WriteTo.Sink(s.GetRequiredService<Plugin>())
                    .CreateLogger())
                .AddTransient<DynamicParser>();

            var provider = collection.BuildServiceProvider();
            if (!KnitServices.ValidateServices(provider))
                throw new InvalidOperationException("Knit services not prepared correctly");
            return provider;
        }

#region IDestructuringPolicy implementation
        bool IDestructuringPolicy.TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [MaybeNullWhen(false)] out LogEventPropertyValue result)
        {
            if (value is ContainerElement container)
            {
                result = propertyValueFactory.CreatePropertyValue(new { Type = container.GetType(), Children = container.ToArray() }, true);
                return true;
            }
            if (value is Element element)
            {
                result = propertyValueFactory.CreatePropertyValue(new { Type = element.GetType() }, true);
                return true;
            }

            result = null;
            return false;
        }
#endregion

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
                LogEventLevel.Verbose => IPALogger.Level.Debug,
                LogEventLevel.Debug => IPALogger.Level.Debug,
                LogEventLevel.Information => IPALogger.Level.Info,
                LogEventLevel.Warning => IPALogger.Level.Warning,
                LogEventLevel.Error => IPALogger.Level.Error,
                LogEventLevel.Fatal => IPALogger.Level.Critical,
                _ => Do(() => ipaLogger.Warn($"Invalid Serilog level {logEvent.Level}"), IPALogger.Level.Info),
            };
            string prefix = "";

            if (logEvent.Properties.TryGetValue("ForPlugin", out var value)
                 && value is ScalarValue scalar
                 && scalar.Value is PluginMetadata meta)
            {
                prefix += $"[{meta.Name}] ";
            }

            if (logEvent.Properties.TryGetValue("SourceContext", out value)
                 && value is ScalarValue scalar2
                 && scalar2.Value is string scontext)
            {
                prefix += $"{{{scontext}}}: ";
            }

            ipaLogger.Log(level, prefix + logEvent.RenderMessage());
            if (logEvent.Exception != null)
                ipaLogger.Log(level, logEvent.Exception);
        }
#endregion
    }
}
