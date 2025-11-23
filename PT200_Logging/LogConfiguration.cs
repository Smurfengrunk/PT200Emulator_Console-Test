using Serilog;

namespace PT200_Logging
{
    public static class PT200_LoggingConfiguration
    {
        public static ILogger CreateLogger()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .CreateLogger();
        }
    }
}