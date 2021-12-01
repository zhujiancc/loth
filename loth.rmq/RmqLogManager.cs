using System;
using System.Configuration;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace loth.rmq
{
    internal class RmqLogManager
    {
        public static LogFactory Instance
        {
            get
            {
                return RmqLogManager._instance.Value;
            }
        }

        private static LogFactory BuildLogFactory()
        {
            LogFactory logFactory = new LogFactory();
            string text = AppDomain.CurrentDomain.BaseDirectory;
            LoggingConfiguration loggingConfiguration = new LoggingConfiguration();
            ColoredConsoleTarget coloredConsoleTarget = new ColoredConsoleTarget();
            loggingConfiguration.AddTarget("console", coloredConsoleTarget);
            coloredConsoleTarget.Layout = "${message}";
            FileTarget fileTarget = new FileTarget();
            loggingConfiguration.AddTarget("file", fileTarget);
            fileTarget.FileName = text.TrimEnd(new char[]
            {
                '\\',
                '/'
            }) + "/Rmqlog${date:format=yyyyMMdd}/rmqlog_${date:format=HH}.txt";
            fileTarget.Layout = "${message}";
            LoggingRule item = new LoggingRule("*", LogLevel.Error, coloredConsoleTarget);
            loggingConfiguration.LoggingRules.Add(item);

            logFactory.Configuration = loggingConfiguration;
            return logFactory;
        }

        private static Lazy<LogFactory> _instance = new Lazy<LogFactory>(new Func<LogFactory>(BuildLogFactory));
    }
}
