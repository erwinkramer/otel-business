using Microsoft.Extensions.Logging;

public partial class Common
{
    public static async Task PerformTomatoAuditExternal(HttpClient httpClient, ILogger logger)
    {
        var response = await httpClient.GetAsync("https://func-otel.azurewebsites.net/api/TomatoTrenches?");

        logger.Log(response.IsSuccessStatusCode ? LogLevel.Information : LogLevel.Error,
            "GET request {status}. Response: {content}",
            response.IsSuccessStatusCode ? "successful" : "failed",
            response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : response.StatusCode.ToString());
    }
}