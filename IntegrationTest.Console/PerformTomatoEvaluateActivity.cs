
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

        using (var activity = businessActivitySource.StartChildBusinessActivity($"Analyzing {state} Price"))
        {
            activity.SetBaggage("Tomato Pricing State", state.ToString());
            activity.SetBaggage("Tomato Price", price.ToString());
            activity.AddEvent(BusinessTracing.NewBusinessEvent("Checking cost of tomato",
            [
                new KeyValuePair<string, object?>("Checker", "Erwin"),
            ]));

            if (price < 4)
            {
                logger.LogBusinessInformation("This is good, because {Business Reason}.", "Tomato is vibing hard!?");
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                try
                {
                    throw new Exception("Yup, this tomato is going to the exception bin.");
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddException(ex);
                }
            }
        }

        if (failFast)
            Environment.FailFast("Triggered a fail fast exception that should not be caught.");

    }
}