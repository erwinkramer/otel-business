using Azure.Messaging.ServiceBus;
using Guanchen.Monitor;
using Microsoft.Extensions.Logging;
using Azure.Core;

public partial class Common
{
    public static async Task PerformTomatoServiceBusRequest(string tomatoId, ILogger logger, TokenCredential cred, string servicebusNamespace)
    {
        const int numOfMessages = 3;

        var clientOptions = new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets };
        ServiceBusClient client = new ServiceBusClient(servicebusNamespace, cred, clientOptions);
        ServiceBusSender sender = client.CreateSender("tomato");

        using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

        for (int i = 1; i <= numOfMessages; i++)
        {
            logger.LogBusinessInformation("Lining up tomato part nr. {Tomato Part ID}.", i);

            if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i} for tomato {tomatoId}")))
            {
                throw new Exception($"The message {i} is too large to fit in the batch.");
            }
        }

        try
        {
            await sender.SendMessagesAsync(messageBatch);
            Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
        }
        finally
        {
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }

    }
}
