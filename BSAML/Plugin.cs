using IPA;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;

namespace BSAML
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin : ILogEventSink
    {
        private readonly IPALogger logger;

        [Init]
        public Plugin(IPALogger logger)
        {
            this.logger = logger;
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
            if (logEvent.Properties.TryGetValue("SourceContext", out var value))
                prefix = $"{{{value}}}: ";
            logger.Log(level, prefix + logEvent.RenderMessage());
            if (logEvent.Exception != null)
                logger.Log(level, logEvent.Exception);
        }
        #endregion
    }
}
