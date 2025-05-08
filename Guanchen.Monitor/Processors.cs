using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Guanchen.Monitor
{
    public class BusinessTracingActivitiyProcessor : BaseProcessor<Activity>
    {
        public override void OnEnd(Activity activity)
        {
            if (activity is null || activity.GetTagItem(BusinessTracing.BusinessTraceTag) is null)
                return;

            foreach (var baggage in BaggageHelper.MergeBaggage(default))
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
            if (record.Attributes == null || !record.Attributes.Any(attr => attr.Key == BusinessTracing.BusinessTraceTag))
                return;

            record.Attributes = BaggageHelper.MergeBaggage(record.Attributes);
        }
    }
}
