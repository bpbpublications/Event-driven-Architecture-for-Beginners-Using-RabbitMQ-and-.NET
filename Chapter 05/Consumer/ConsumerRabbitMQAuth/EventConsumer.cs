using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitMQAuthenticationExample
{
    public class EventConsumer
    {
        private readonly string _hostname = "localhost";
        private readonly int _port = 5672;
        private readonly string _username = "guest";
        private readonly string _password = "guest";
        private readonly string _exchangeName = "my_exchange";
        private readonly string _queueName = "my_queue";
        private readonly string _routingKey = "my_routing_key";

        public void ConsumeEvents()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                Port = _port,
                UserName = _username,
                Password = _password
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(
                    exchange: _exchangeName,
                    type: ExchangeType.Direct
                );

                channel.QueueDeclare(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                channel.QueueBind(
                    queue: _queueName,
                    exchange: _exchangeName,
                    routingKey: _routingKey
                );

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, eventArgs) =>
                {
                    var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                    Console.WriteLine("Received event: " + message);
                };

                channel.BasicConsume(
                    queue: _queueName,
                    autoAck: true,
                    consumer: consumer
                );

                Console.WriteLine("Waiting for events...");
            }
        }
    }
}
