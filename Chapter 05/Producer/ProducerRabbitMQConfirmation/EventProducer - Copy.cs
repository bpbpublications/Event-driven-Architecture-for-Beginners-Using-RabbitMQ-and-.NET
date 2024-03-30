using RabbitMQ.Client;
using System;
using System.Text;

public class EventProducer
{
    private readonly string _rabbitMqConnectionString;

    public EventProducer(string rabbitMqConnectionString)
    {
        _rabbitMqConnectionString = rabbitMqConnectionString;
    }

    public void PublishEvent(string exchangeName, string routingKey, string message)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_rabbitMqConnectionString) };

        using (var connection = factory.CreateConnection()) 
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);
            
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: exchangeName,
                                 routingKey: routingKey,
                                 basicProperties: null,
                                 body: body);
            
            Console.WriteLine("Event published: {0}", message);
        }
    }
}
