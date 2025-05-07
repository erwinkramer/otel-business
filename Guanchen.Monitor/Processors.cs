using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Guanchen.Monitor
{
    public class BusinessTracingActivitiyProcessor : BaseProcessor<Activity>
    {
        public override void OnStart(Activity activity)
        {
            if (activity is null) return;

            foreach (var baggage in Baggage.GetBaggage())
            {
                activity.SetTag(baggage.Key, baggage.Value);
            }
        }
    }

    public class BusinessTracingLogProcessor : BaseProcessor<LogRecord>
    {
        private readonly string name;

        public BusinessTracingLogProcessor(string name = "Guanchen.Monitor.BusinessTracingLogProcessor")
        {
            this.name = name;
        }

        public override void OnEnd(LogRecord record)
        {
            var mergedAttributes = BaggageHelper.MergeBaggage(record.Attributes);
            record.Attributes = mergedAttributes;
        }
    }
}
