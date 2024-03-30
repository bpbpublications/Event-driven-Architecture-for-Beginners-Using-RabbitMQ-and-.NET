using System;
using RabbitMQ.Client;
using System.Text;

namespace EventProducer
{
    public class Producer
    {
        private readonly string _exchangeName;
        private readonly string _routingKey;
        private readonly string _rabbitMQConnectionString;

        public Producer(string exchangeName, string routingKey, string rabbitMQConnectionString)
        {
            _exchangeName = exchangeName;
            _routingKey = routingKey;
            _rabbitMQConnectionString = rabbitMQConnectionString;
        }

        // Authorization function for publishing events
        public bool IsAuthorizedToPublish(CurrentUser user)
        {
            // Implement your authorization logic here based on user's identity or role
            // Return true if the user is authorized to publish events, otherwise return false
            // You can use JWT tokens, user roles, or any other authentication mechanism to check authorization
            return user.HasRole("publisher");
        }

        public void PublishEvent(string message, CurrentUser user)
        {
            try
            {
                // Perform authorization check
                if (!IsAuthorizedToPublish(user))
                {
                    Console.WriteLine("Not authorized to publish events.");
                    return;
                }

                var factory = new ConnectionFactory()
                {
                    Uri = new Uri(_rabbitMQConnectionString),
                    DispatchConsumersAsync = true
                };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic, durable: true, autoDelete: false);

                    var body = Encoding.UTF8.GetBytes(message);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true; // Make the message persistent

                    channel.BasicPublish(exchange: _exchangeName,
                                         routingKey: _routingKey,
                                         basicProperties: properties,
                                         body: body);

                    Console.WriteLine(" [x] Sent '{0}':'{1}'", _routingKey, message);
                }
            }
            catch (Exception ex)


            {
                Console.WriteLine($"Error publishing event: {ex.Message}");
            }
        }
    }
}
