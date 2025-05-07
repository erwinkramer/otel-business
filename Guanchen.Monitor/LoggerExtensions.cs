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

            tagsCollection.TryAdd(BusinessInformationScopeTag.Key, BusinessInformationScopeTag.Value);

            // Merge additional baggage and add it to tagsCollection
            var mergedTags = BaggageHelper.MergeBaggage(tags);
            foreach (var kvp in mergedTags)
            {
                tagsCollection.TryAdd(kvp.Key, kvp.Value);
            }

            return new ActivityEvent(businessEvent, default, tagsCollection);
        }
    }
}
