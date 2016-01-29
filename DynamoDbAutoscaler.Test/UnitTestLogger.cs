using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace DynamoDbAutoscaler.Test
{
   public class UnitTestLogger
    {
        private static ILogger logger;
        public static ILogger Logger
        {
            get
            {
                if(logger == null)
                {
                    logger = CreateLogger();
                }

                return logger;
            }
        }

        public static ILogger CreateLogger()
        {
            var config = new LoggerConfiguration();
            config
                .MinimumLevel.ControlledBy(new LoggingLevelSwitch { MinimumLevel = LogEventLevel.Debug })
                .WriteTo.ColoredConsole()
                .WriteTo.Trace();

            return config.CreateLogger();
        }
    }
}
