
using Guanchen.Monitor;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTelemetry;

public partial class Common
{

    public enum TomatoPricingState
    {
        Current,
        Future
    }

    public static async Task PerformTomatoEvaluateActivity(TomatoPricingState state, string tomatoId, ILogger logger, ActivitySource businessActivitySource, bool failFast = false)
    {
        var price = Math.Round(new Random().NextDouble() * 20, 2);

        Baggage.SetBaggage("Tomato Pricing State", state.ToString());
        Baggage.SetBaggage("Tomato Price", price.ToString());

        using (var activity = businessActivitySource.StartChildBusinessActivity($"Analyzing {state} Price"))
        {
            activity.AddEvent(BusinessTracing.NewBusinessEvent("Checking cost of tomato",
            [
                new KeyValuePair<string, object?>("Checker", "Erwin"),
            ]));

            if (price < 18)
            {
                logger.LogBusinessInformation("This is good, because {Business Reason}.", "Tomato is vibing hard!?");
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                logger.LogBusinessError("An error occurred, because {Business Reason}.", "Tomato is capping hard!?");
                activity?.SetStatus(ActivityStatusCode.Error);
            }
        }

        if (failFast)
            Environment.FailFast("Triggered a fail fast exception that should not be caught.");

    }
}