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
            bool hasAttributes = record.Attributes?.Count > 0;
            bool hasActivityBaggage = Activity.Current?.Baggage is { } activityBaggage && activityBaggage.GetEnumerator().MoveNext();
            bool hasGlobalBaggage = Baggage.GetBaggage().GetEnumerator().MoveNext();

            if (!hasActivityBaggage && !hasGlobalBaggage) return;

            var mergedAttributes = hasAttributes
                ? new List<KeyValuePair<string, object?>>(record.Attributes!)
                : new List<KeyValuePair<string, object?>>();

            if (hasActivityBaggage)
            {
                foreach (var kvp in Activity.Current!.Baggage)
                {
                    mergedAttributes.Add(new KeyValuePair<string, object?>(kvp.Key, kvp.Value));
                }
            }

            if (hasGlobalBaggage)
            {
                foreach (var kvp in Baggage.GetBaggage())
                {
                    mergedAttributes.Add(new KeyValuePair<string, object?>(kvp.Key, kvp.Value));
                }
            }

            record.Attributes = mergedAttributes;
        }
    }
}
