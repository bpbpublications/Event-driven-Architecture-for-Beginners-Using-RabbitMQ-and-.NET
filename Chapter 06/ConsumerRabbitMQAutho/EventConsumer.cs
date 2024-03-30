using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventConsumer
{
    public class EventConsumer
    {
        private readonly string _queueName;
        private readonly string _rabbitMQConnectionString;

        public EventConsumer(string queueName, string rabbitMQConnectionString)
        {
            _queueName = queueName;
            _rabbitMQConnectionString = rabbitMQConnectionString;
        }

        public void StartConsuming()
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(_rabbitMQConnectionString)
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine("Received message: " + message);

                        // Implement your authorization logic here
                        if (IsAuthorizedToProcessMessage(ea.BasicProperties.UserId))
                        {
                            // Process the message here
                            Console.WriteLine("Message processed successfully.");
                            channel.BasicAck(ea.DeliveryTag, multiple: false); // Acknowledge message
                        }
                        else
                        {
                            // Reject or requeue the message if not authorized
                            Console.WriteLine("Unauthorized to process the message. Rejecting it.");
                            channel.BasicReject(ea.DeliveryTag, requeue: false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing message: " + ex.Message);
                        channel.BasicReject(ea.DeliveryTag, requeue: false);
                    }
                };

                channel.BasicConsume(_queueName, autoAck: false, consumer);
                Console.WriteLine("Consumer started. Waiting for messages...");
                Console.ReadLine(); // Keep the application running until manually stopped.
            }
        }

        private bool IsAuthorizedToProcessMessage(string userId)
        {
            // Implement your custom authorization logic here based on the user's identity (userId).
            // You can check the user's roles, permissions, or any other criteria to determine authorization.

            // For demonstration purposes, let's assume that any user with a specific role ("consumer") is authorized.
            return HasRole(userId, "consumer");
        }

        private bool HasRole(string userId, string roleName)
        {
            // Implement your logic to check if the user with the given userId has the specified role (roleName).
            // This could involve querying a database or an authentication/authorization service.
            // For simplicity, we'll just check if the role is "consumer" in this example.
            return roleName.ToLower() == "consumer";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string queueName = "my_event_queue";
            string rabbitMQConnectionString = "amqp://my_consumer:password@localhost:5672/my_virtual_host";

            var consumer = new EventConsumer(queueName, rabbitMQConnectionString);
            consumer.StartConsuming();
        }
    }
}
