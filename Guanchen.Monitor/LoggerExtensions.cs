using Microsoft.Extensions.Logging;

namespace Guanchen.Monitor
{
    public static partial class BusinessTracing
    {
        public static void LogBusinessInformation(this ILogger logger, string message, params object[] args)
        {
            // For short messages, string interpolation is faster than StringBuilder
            var businessMessage = $"{BusinessMessagePrefix}{message}";

            // Avoids allocating a new dictionary every time
            using (logger.BeginScope(BusinessInformationScope))
            {
                logger.LogInformation(businessMessage, args);
            }
        }

        public static void LogBusinessError(this ILogger logger, string message, params object[] args)
        {
            // For short messages, string interpolation is faster than StringBuilder
            var businessMessage = $"{BusinessMessagePrefix}{message}";

            // Avoids allocating a new dictionary every time
            using (logger.BeginScope(BusinessErrorScope))
            {
                logger.LogInformation(businessMessage, args);
            }
        }
    }
}
