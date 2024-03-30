using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using Serilog.Sinks.Splunk;

namespace EventDrivenLoggingExample
{
    public class Producer
    {
        private readonly string _queueName;
        private readonly string _rabbitMQConnectionString;

        public Producer(string queueName, string rabbitMQConnectionString)
        {
            _queueName = queueName;
            _rabbitMQConnectionString = rabbitMQConnectionString;
        }

        public void PublishEvent(string message)
        {
            try
            {
                var factory = new ConnectionFactory() { Uri = new Uri(_rabbitMQConnectionString) };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);

                    var body = Encoding.UTF8.GetBytes(message);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true; // Make the message persistent

                    channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: properties, body: body);

                    Console.WriteLine(" [x] Sent message: " + message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error publishing event: " + ex.Message);
            }
        }
    }

    public class Consumer
    {
        private readonly string _queueName;
        private readonly string _rabbitMQConnectionString;

        public Consumer(string queueName, string rabbitMQConnectionString)
        {
            _queueName = queueName;
            _rabbitMQConnectionString = rabbitMQConnectionString;
        }

        public void StartConsuming()
        {
            var factory = new ConnectionFactory() { Uri = new Uri(_rabbitMQConnectionString) };
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

                        // Log the received message using Serilog
                        Log.Information("Received message: {Message}", message);

                        channel.BasicAck(ea.DeliveryTag, multiple: false); // Acknowledge message
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
    }

    class Program
    {
        static void Main(string[] args)
        {
            string queueName = "my_event_queue";
            string rabbitMQConnectionString = "amqp://guest:guest@localhost:5672/";

            // Setup Serilog logger to log to Splunk
            Log.Logger = new LoggerConfiguration()
                .WriteTo.EventCollector("http://localhost:8088", "my-token")
                .CreateLogger();

            var producer = new Producer(queueName, rabbitMQConnectionString);
            producer.PublishEvent("Hello, this is a log message from the producer!");

            var consumer = new Consumer(queueName, rabbitMQConnectionString);
            consumer.StartConsuming();

            // Flush the logger before exiting the application
            Log.CloseAndFlush();
        }
    }
}
