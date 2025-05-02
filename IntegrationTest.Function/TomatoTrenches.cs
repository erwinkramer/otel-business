using Guanchen.Monitor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Common
{
    public class TomatoTrenches
    {
        private readonly ILogger<TomatoTrenches> _logger;

        public static ActivitySource businessActivitySource = new ActivitySource("Guanchen.InternalServices.Demo.OrderingService");

        public TomatoTrenches(ILogger<TomatoTrenches> logger)
        {
            _logger = logger;
        }

        [Function("TomatoTrenches")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation($"Current Activity: {Activity.Current?.DisplayName}, TraceId: {Activity.Current?.Id}, SpanId: {Activity.Current?.SpanId}, RootId: {Activity.Current?.RootId}");

            using (var activity = businessActivitySource.StartChildBusinessActivity("Auditing HTTP Tomato"))
            {
                _logger.LogBusinessInformation("Welcome to the trenches, buckle up tomato.");
                _logger.LogBusinessInformation("Hmmm, have you been a good tomato? Let's find out.");
                _logger.LogBusinessInformation("Seems like a good tomato after all.");
            }
            return new OkObjectResult("Hope you had a good stay in the trenches, tomato.");
        }

        [Function("TomatoQueueTrigger")]
        public void ProcessServiceBusMessage(
            [ServiceBusTrigger("tomato", Connection = "ServiceBusConnection")] string message)
        {
            _logger.LogInformation($"Current Activity: {Activity.Current?.DisplayName}, TraceId: {Activity.Current?.Id}, SpanId: {Activity.Current?.SpanId}, RootId: {Activity.Current?.RootId}");

            using (var activity = businessActivitySource.StartChildBusinessActivity("Auditing Queue Tomato"))
            {
                _logger.LogBusinessInformation($"Welcome to the bus, tomato message: {message}");
                _logger.LogBusinessInformation("They are back, the tomatoes are back. This time they are in a queue.");
                _logger.LogBusinessInformation("This queue tomato is kinda sus.");
            }
        }
    }
}
