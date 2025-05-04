using Azure.Identity;
using Guanchen.Monitor;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;

public class Program
{
    public static async Task Main(string[] args)
    {
        var failFast = args.Length > 0 && args[0] == "true";

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_TENANT_ID")) ||
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
            throw new InvalidOperationException("Required environment variables 'AZURE_TENANT_ID' and/or 'APPLICATIONINSIGHTS_CONNECTION_STRING' are not set.");

        var servicebusNamespace = Environment.GetEnvironmentVariable("AZURE_SERVICEBUS_FULLYQUALIFIEDNAMESPACE")
            ?? throw new InvalidOperationException("Environment variable 'AZURE_SERVICEBUS_FULLYQUALIFIEDNAMESPACE' is not set.");

        var serviceName = "Tomato Pricer";
        var cred = new DefaultAzureCredential(new DefaultAzureCredentialOptions { });

        using ActivitySource businessActivitySource = new ActivitySource("Guanchen.InternalServices.Demo.TomatoService", "2.0.0");
        using var loggerFactory = BusinessTracing.CreateLoggerFactory();
        using var meterProvider = BusinessTracing.CreateBusinessMetricProviderBuilder(serviceName).Build();
        using var tracerProvider = businessActivitySource.CreateBusinessTracerProviderBuilder(serviceName).Build();
        using var httpClient = new HttpClient();

        var logger = loggerFactory.CreateLogger(businessActivitySource);

        try
        {
            await Common.ProcessTomatoBatchAsync(businessActivitySource, logger, httpClient, cred, servicebusNamespace, failFast);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong while processing tomatoes.");
        }

        Console.WriteLine("Completed 😎");
    }
}
