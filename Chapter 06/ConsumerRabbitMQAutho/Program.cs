using System;

namespace RabbitMQAuthenticationExample
{
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
