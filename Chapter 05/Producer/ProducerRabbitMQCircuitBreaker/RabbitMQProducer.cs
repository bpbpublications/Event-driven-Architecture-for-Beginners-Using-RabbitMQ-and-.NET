using System;
using System.Text;
using Polly;
using RabbitMQ.Client;

public class RabbitMQProducer
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly Policy _circuitBreakerPolicy;

    public RabbitMQProducer()
    {
        _connectionFactory = new ConnectionFactory()
        {
            // Configure RabbitMQ connection parameters
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        _circuitBreakerPolicy = Policy.Handle<Exception>()
            .CircuitBreaker(3, TimeSpan.FromSeconds(30), (ex, ts) =>
            {
                // Circuit breaker open action
                Console.WriteLine("Circuit breaker opened due to exception: " + ex.Message);
            }, () =>
            {
                // Circuit breaker reset action
                Console.WriteLine("Circuit breaker reset");
            });
    }

    public void PublishMessage(string message)
    {
        _circuitBreakerPolicy.Execute(() =>
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var body = Encoding.UTF8.GetBytes(message);

                channel.QueueDeclare(queue: "myqueue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.BasicPublish(exchange: "",
                                     routingKey: "myqueue",
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine("Message published: " + message);
            }
        });
    }
}
