using System;

namespace ProducerRabbitMQ
{
    class Program
    {
        static void Main(string[] args)
        {
            var rabbitMqConnectionString = "amqp://guest:guest@localhost:5672/";
            var exchangeName = "my_exchange";
            var routingKey = "my_event";
            var message = "Hello, RabbitMQ!";
            var maxRetries = 3;
            var delayBetweenRetries = TimeSpan.FromSeconds(5);

            var producer = new EventProducer(rabbitMqConnectionString, exchangeName);
            producer.PublishEventWithRetry(routingKey, message, maxRetries, delayBetweenRetries);
        }

    }
}
