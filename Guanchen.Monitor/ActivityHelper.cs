using System.Diagnostics;
using OpenTelemetry;

namespace Guanchen.Monitor
{
    public static partial class BusinessTracing
    {
        public class LinkedActivity : IDisposable
        {
            public Activity Activity { get; }
            public Activity ParentActivity { get; }

            public LinkedActivity(Activity activity, Activity parentActivity)
            {
                Activity = activity;
                ParentActivity = parentActivity;
            }

            private bool disposedValue;
            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        Activity.Dispose();
                        Activity.Current = ParentActivity;
                    }

                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        public static Activity StartParentBusinessActivity(this ActivitySource activitySource, string activityName)
        {
            if (!activitySource.HasListeners())
                throw new InvalidOperationException($"No listeners are attached to the ActivitySource '{activitySource.Name}'.");

            var context = new ActivityContext(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded);
            return StartChildBusinessActivity(activitySource, activityName, null, context);
        }

        /// <summary>
        /// Starts a linked activity with a new operation ID
        /// Setup taken from https://gist.github.com/arakis/6063fc4c263ef797651c7066d6a47870 because of current issue https://github.com/open-telemetry/opentelemetry-dotnet/issues/984
        /// </summary>
        public static LinkedActivity StartLinkedBusinessActivity(this ActivitySource activitySource, string activityName)
        {
             if (!activitySource.HasListeners())
                throw new InvalidOperationException($"No listeners are attached to the ActivitySource '{activitySource.Name}'.");

            var parent = Activity.Current;

            if(parent == null)
                throw new InvalidOperationException("No parent activity is available.");

            Activity.Current = null;

            var parentLinks = new[] { new ActivityLink(parent.Context) };
            var next = StartChildBusinessActivity(activitySource, activityName, parentLinks);

            return new LinkedActivity(next, parent);
        }

        public static Activity StartChildBusinessActivity(
            this ActivitySource activitySource,
            string activityName,
            IEnumerable<ActivityLink>? activityLinks = null,
            ActivityContext context = default)
        {
            if (!activitySource.HasListeners())
                throw new InvalidOperationException($"No listeners are attached to the ActivitySource '{activitySource.Name}'.");

            string businessActivityName = $"💼 {activityName}";

            var activity = activitySource.StartActivity(
                businessActivityName,
                ActivityKind.Server,
                context,
                tags: null,
                links: activityLinks);

            activity?.SetTag("Business Trace", "Information");
            Baggage.SetBaggage("Business Span Id", activity?.SpanId.ToString());

            return activity ?? throw new InvalidOperationException($"Failed to start activity '{businessActivityName}'.");
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
