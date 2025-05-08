using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using OpenTelemetry.Metrics;

namespace Guanchen.Monitor
{
    public static partial class BusinessTracing
    {
        public static MeterProviderBuilder CreateBusinessMetricProviderBuilder(string serviceName)
        {
            return Sdk.CreateMeterProviderBuilder()
                .AddMeter(serviceName)
                .AddConsoleExporter()
                .AddAzureMonitorMetricExporter();
        }

        /// <summary>
        /// Suitable for console applications
        /// </summary>
        public static TracerProviderBuilder CreateBusinessTracerProviderBuilder(this ActivitySource businessActivitySource, string serviceName)
        {
            ConfigurePropagators();

            return ConfigureCommonTracing(
                Sdk.CreateTracerProviderBuilder(),
                businessActivitySource,
                serviceName,
                includeConsoleExporter: true
            );
        }

        /// <summary>
        /// Suitable for Azure Functions
        /// </summary>
        public static IServiceCollection AddBusinessTracerService(this IServiceCollection services, ActivitySource businessActivitySource, string serviceName)
        {
            ConfigurePropagators();

            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    ConfigureCommonTracing(builder, businessActivitySource, serviceName, includeConsoleExporter: false);
                })
                .UseAzureMonitor()
                .UseFunctionsWorkerDefaults();

            return services;
        }

        /// <summary>
        /// Shared propagator setup
        /// </summary>
        private static void ConfigurePropagators()
        {
            var otelPropagator = new CompositeTextMapPropagator(
                [
                    new TraceContextPropagator(),
                    new BaggagePropagator(),
                ]);

            Sdk.SetDefaultTextMapPropagator(otelPropagator);
            DistributedContextPropagator.Current = new OTelBridgePropagator(otelPropagator); // this should be done by OTel SDK
        }

        /// <summary>
        /// Common tracing configuration used by various app types
        /// </summary>
        private static TracerProviderBuilder ConfigureCommonTracing(
            TracerProviderBuilder builder,
            ActivitySource businessActivitySource,
            string serviceName,
            bool includeConsoleExporter)
        {
            builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("main", $"🧭 {serviceName}", "2.0.0"))
                .AddProcessor(new BusinessTracingActivitiyProcessor())
                .AddHttpClientInstrumentation()
                .AddSource(businessActivitySource.Name)
                .AddSource("Azure.*")
                .AddSource("Microsoft.Azure.Functions.*");

            if (includeConsoleExporter)
            {
                builder.AddAzureMonitorTraceExporter();
                builder.AddConsoleExporter();
            }

            // always add AutoFlush last
            builder.AddAutoFlushActivityProcessor(a => a.GetTagItem(BusinessTraceTag) != null, 5000); //https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/5aa6d86836bbc13659d61abcf3040a0811537f7e/src/OpenTelemetry.Extensions#autoflushactivityprocessor

            return builder;
        }
    }
}
