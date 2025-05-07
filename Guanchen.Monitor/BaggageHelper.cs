using System.Diagnostics;
using OpenTelemetry;
using System.Linq;

namespace Guanchen.Monitor
{
    public static class BaggageHelper
    {
        public static List<KeyValuePair<string, object?>> MergeBaggage(IEnumerable<KeyValuePair<string, object?>>? additionalBaggage)
        {
            var mergedBaggage = new List<KeyValuePair<string, object?>>();
            var seenKeys = new HashSet<string>(); // To track unique keys in a performant way

            // Add additional baggage first (highest priority)
            if (additionalBaggage != null)
            {
                foreach (var kvp in additionalBaggage)
                {
                    if (!seenKeys.Contains(kvp.Key))
                    {
                        seenKeys.Add(kvp.Key);
                        mergedBaggage.Add(kvp);
                    }
                }
            }

            // Add activity baggage next (medium priority)
            var currentActivity = Activity.Current;
            if (currentActivity != null)
            {
                foreach (var kvp in currentActivity.Baggage)
                {
                    if (!seenKeys.Contains(kvp.Key))
                    {
                        seenKeys.Add(kvp.Key);
                        mergedBaggage.Add(new KeyValuePair<string, object?>(kvp.Key, kvp.Value));
                    }
                }
            }

            // Add global baggage last (lowest priority)
            foreach (var kvp in Baggage.GetBaggage())
            {
                if (!seenKeys.Contains(kvp.Key))
                {
                    seenKeys.Add(kvp.Key);
                    mergedBaggage.Add(new KeyValuePair<string, object?>(kvp.Key, kvp.Value));
                }
            }

            return mergedBaggage;
        }
    }
}
