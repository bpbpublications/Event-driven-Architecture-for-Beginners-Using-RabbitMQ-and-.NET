using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

public class EventProducer
{
    private readonly string rabbitMqConnectionString;
    private readonly string exchangeName;
    private readonly string originalQueueName;
    private readonly string deadLetterExchangeName;
    private readonly string deadLetterQueueName;
    private readonly string routingKey;

    public EventProducer(string rabbitMqConnectionString, string exchangeName, string originalQueueName,
        string deadLetterExchangeName, string deadLetterQueueName, string routingKey)
    {
        this.rabbitMqConnectionString = rabbitMqConnectionString;
        this.exchangeName = exchangeName;
        this.originalQueueName = originalQueueName;
        this.deadLetterExchangeName = deadLetterExchangeName;
        this.deadLetterQueueName = deadLetterQueueName;
        this.routingKey = routingKey;
    }

    public void PublishEvent(string message)
    {
        var factory = new ConnectionFactory { Uri = new Uri(rabbitMqConnectionString) };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // Declare the original queue with DLQ configuration
            channel.QueueDeclare(originalQueueName, durable: true, exclusive: false, autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", deadLetterExchangeName },
                    { "x-dead-letter-routing-key", routingKey }
                });

            // Declare the dead letter exchange and queue
            channel.ExchangeDeclare(deadLetterExchangeName, ExchangeType.Direct, durable: true);
            channel.QueueDeclare(deadLetterQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueBind(deadLetterQueueName, deadLetterExchangeName, routingKey, null);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchangeName, routingKey, properties, body);

            Console.WriteLine("Event published successfully!");
        }
    }
}
