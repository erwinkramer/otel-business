using System.Diagnostics;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

namespace Guanchen.Monitor
{
    public static partial class BusinessTracing
    {
        public static ILoggerFactory CreateLoggerFactory()
        {
            return LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSimpleConsole(options => options.IncludeScopes = true);
                builder.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
                builder.AddOpenTelemetry(options =>
                {
                    ConfigureSharedOpenTelemetryLogger(options);
                    options.AddAzureMonitorLogExporter();
                    options.AddConsoleExporter(); // Console exporter only for standalone logger factory
                });
            });
        }

        public static void ConfigureBusinessLogger(this IServiceCollection services)
        {
            services.Configure<OpenTelemetryLoggerOptions>(ConfigureSharedOpenTelemetryLogger);
        }

        private static void ConfigureSharedOpenTelemetryLogger(OpenTelemetryLoggerOptions options)
        {
            options.IncludeScopes = true;
            options.AddProcessor(new BusinessTracingLogProcessor());
            //options.AttachLogsToActivityEvent(); // Required for the AutoFlushActivityProcessor to make sure that logs are also flushed with the same activity/span
        }

        public static ILogger CreateLogger(this ILoggerFactory loggerFactory, ActivitySource source)
        {
            return loggerFactory.CreateLogger(source.Name);
        }
    }
}
