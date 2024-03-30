using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

public class EventConsumer
{
    private readonly string rabbitMqConnectionString;

    public EventConsumer(string rabbitMqConnectionString)
    {
        this.rabbitMqConnectionString = rabbitMqConnectionString;
    }

    public void ConsumeEvents(string exchangeName, string queueName, string routingKey)
    {
        var factory = new ConnectionFactory { Uri = new Uri(rabbitMqConnectionString) };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic);
            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(queueName, exchangeName, routingKey);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine("Event received: {0}", message);
            };

            channel.BasicConsume(queueName, autoAck: true, consumer: consumer);

            Console.WriteLine("Waiting for events. Press any key to exit.");
            Console.ReadLine();
        }
    }
}
