using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

public class EventProducer1
{
    private readonly string rabbitMqConnectionString;
    private readonly string exchangeName;

    public EventProducer1(string rabbitMqConnectionString, string exchangeName)
    {
        this.rabbitMqConnectionString = rabbitMqConnectionString;
        this.exchangeName = exchangeName;
    }

    public void PublishEventWithRetry(string routingKey, string message, int maxRetries, TimeSpan delayBetweenRetries)
    {
        var factory = new ConnectionFactory { Uri = new Uri(rabbitMqConnectionString) };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            var retryCount = 0;
            var isPublished = false;

            while (retryCount <= maxRetries && !isPublished)
            {
                try
                {
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchangeName, routingKey, properties, body);
                    Console.WriteLine("Event published successfully!");
                    isPublished = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to publish event: {ex.Message}");
                    retryCount++;
                    Thread.Sleep(delayBetweenRetries);
                }
            }

            if (!isPublished)
            {
                Console.WriteLine("Failed to publish event after maximum retries.");
                // Optionally, you can log or handle the failure in a specific way.
            }
        }
    }
}
