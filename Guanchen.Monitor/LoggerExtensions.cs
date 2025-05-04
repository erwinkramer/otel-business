using System.Diagnostics;
using Microsoft.Extensions.Logging;

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
            // For short messages, string interpolation is faster than StringBuilder
            var businessEvent = $"💼 {message}";

            // Avoid unnecessary allocations by initializing with capacity if tags are provided
            var tagsCollection = tags is not null 
            ? new ActivityTagsCollection(tags) 
            : new ActivityTagsCollection();

            // Add the constant tag directly
            tagsCollection.Add(BusinessInformationScopeTag);

            return new ActivityEvent(businessEvent, default, tagsCollection);
        }
    }
}
