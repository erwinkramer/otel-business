using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTelemetry;

namespace Guanchen.Monitor
{
    public static partial class BusinessTracing
    {
        private static readonly KeyValuePair<string, object?> BusinessInformationScopeTag =
            new("Business Trace", "Information");

        private static readonly KeyValuePair<string, object?> BusinessErrorScopeTag =
            new("Business Trace", "Error");

        private static readonly IReadOnlyList<KeyValuePair<string, object?>> BusinessInformationScope =
            new[] { BusinessInformationScopeTag };

        private static readonly IReadOnlyList<KeyValuePair<string, object?>> BusinessErrorScope =
            new[] { BusinessErrorScopeTag };

        public static void LogBusinessInformation(this ILogger logger, string message, params object[] args)
        {
            // For short messages, string interpolation is faster than StringBuilder
            var businessMessage = $"💼 {message}";

            // Avoids allocating a new dictionary every time
            using (logger.BeginScope(BusinessInformationScope))
            {
                logger.LogInformation(businessMessage, args);
            }
        }

        public static void LogBusinessError(this ILogger logger, string message, params object[] args)
        {
            // For short messages, string interpolation is faster than StringBuilder
            var businessMessage = $"💼 {message}";

            // Avoids allocating a new dictionary every time
            using (logger.BeginScope(BusinessErrorScope))
            {
                logger.LogInformation(businessMessage, args);
            }
        }

        public static ActivityEvent NewBusinessEvent(string message, IEnumerable<KeyValuePair<string, object?>>? tags = null)
        {
            var businessEvent = $"💼 {message}";

            // Initialize tag collection
            var tagsCollection = tags is ICollection<KeyValuePair<string, object?>> collection
                ? new ActivityTagsCollection(collection)
                : new ActivityTagsCollection();

            // Track keys to prevent duplicates
            HashSet<string>? seenKeys = null;

            if (tags != null)
            {
                seenKeys = new HashSet<string>();
                foreach (var kvp in tags)
                {
                    seenKeys.Add(kvp.Key);
                }
            }

            // Always add the constant tag if not already present
            seenKeys ??= new HashSet<string>();
            if (seenKeys.Add(BusinessInformationScopeTag.Key))
            {
                tagsCollection.Add(BusinessInformationScopeTag);
            }

            // Add Activity.Current baggage
            var currentActivity = Activity.Current;
            if (currentActivity != null)
            {
                foreach (var kvp in currentActivity.Baggage)
                {
                    if (seenKeys.Add(kvp.Key))
                    {
                        tagsCollection.Add(new KeyValuePair<string, object?>(kvp.Key, kvp.Value));
                    }
                }
            }

            // Add global baggage
            foreach (var kvp in Baggage.GetBaggage())
            {
                if (seenKeys.Add(kvp.Key))
                {
                    tagsCollection.Add(new KeyValuePair<string, object?>(kvp.Key, kvp.Value));
                }
            }

            return new ActivityEvent(businessEvent, default, tagsCollection);
        }
    }
}
