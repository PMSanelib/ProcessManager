using System;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace ProcessManager
{
    public class LogHelper
    {
       
        public static void AddLogger(string loggerName)
        {
            var configuration = ServiceConfigurationProvider.Instance;

            if (LogManager.Configuration == null)
            {
                LogManager.Configuration = new LoggingConfiguration();
            }

            var loggingConfiguration = LogManager.Configuration;

            var fileTarget = new FileTarget
            {
                Encoding = Encoding.UTF8,
                FileName = $"{configuration.TempPath}/Logs/{loggerName}-{DateTime.Now.ToString("dd-MM-yyyy_hhmmss")}.txt",
                Layout = "${date:format=HH\\:mm\\:ss} ${logger} ${event-context:item=Dev} [${level:uppercase=true}]\t${message}.  ${exception:format=ToString,StackTrace}"
            };

            loggingConfiguration.AddTarget("file", fileTarget);

            var async = new AsyncTargetWrapper(fileTarget, 5000, AsyncTargetWrapperOverflowAction.Block);
            loggingConfiguration.LoggingRules.Add(new LoggingRule(loggerName, LogLevel.Debug, async));

            LogManager.Configuration = loggingConfiguration;

            LogManager.Configuration.Reload();
        }
    }
}