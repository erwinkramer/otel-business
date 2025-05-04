
using Azure.Core;
using Guanchen.Monitor;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using System.Diagnostics;

public partial class Common
{
    public static async Task ProcessTomatoBatchAsync(ActivitySource businessActivitySource, ILogger logger, HttpClient httpClient, TokenCredential cred, string servicebusNamespace, bool failFast = false)
    {
        Baggage.SetBaggage("Batch Date", DateTime.UtcNow.ToString("o"));

        using (var batchActivity = businessActivitySource.StartParentBusinessActivity("Splitting Tomato Batch"))
        {
            batchActivity.AddEvent(BusinessTracing.NewBusinessEvent("Will be processing individual tomatoes."));
            logger.LogBusinessInformation("Another day, another batch of tomaders, am I right?");

            for (int i = 0; i < 3; i++)
            {
                var tomatoId = Guid.NewGuid().ToString(); // Simulate a tomato ID
                Baggage.SetBaggage("Tomato ID", tomatoId);

                using (var tomatoActivity = businessActivitySource.StartLinkedBusinessActivity("Evaluating Tomato"))
                {
                    logger.LogBusinessInformation("Every tomato matters.");

                    await PerformTomatoEvaluateActivity(TomatoPricingState.Current, tomatoId, logger, businessActivitySource, failFast);
                    await PerformTomatoEvaluateActivity(TomatoPricingState.Future, tomatoId, logger, businessActivitySource, failFast);

                    await PerformTomatoAuditExternal(httpClient, logger);
                    await PerformTomatoServiceBusRequest(tomatoId, logger, cred, servicebusNamespace);
                }
            }
        }
    }
}