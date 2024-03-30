using RabbitMQ.Client;
using System;
using System.Text;

namespace RabbitMQAuthenticationExample
{
    public class EventProducer
    {
        private readonly string _hostname = "localhost";
        private readonly int _port = 5672;
        private readonly string _username = "guest";
        private readonly string _password = "guest";
        private readonly string _exchangeName = "my_exchange";
        private readonly string _routingKey = "my_routing_key";

        public void PublishMessage(string message)
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
                var messageBody = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: _routingKey,
                    basicProperties: null,
                    body: messageBody
                );
            }
        }
    }
}
