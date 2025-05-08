namespace Guanchen.Monitor
{
    public static partial class BusinessTracing
    {
        public const string BusinessTraceTag = "Business Trace";

        public const string BusinessMessagePrefix = "💼 ";

        private static readonly KeyValuePair<string, object?> BusinessInformationScopeTag =
           new(BusinessTraceTag, "Information");

        private static readonly KeyValuePair<string, object?> BusinessErrorScopeTag =
            new(BusinessTraceTag, "Error");

        private static readonly IReadOnlyList<KeyValuePair<string, object?>> BusinessInformationScope =
            new[] { BusinessInformationScopeTag };

        private static readonly IReadOnlyList<KeyValuePair<string, object?>> BusinessErrorScope =
            new[] { BusinessErrorScopeTag };
    }
}