namespace ProducerRabbitMQ
{
    class Program
    {
        static void Main(string[] args)
        {
            var rabbitMqConnectionString = "amqp://guest:guest@localhost:5672/";
            var exchangeName = "my_exchange";
            var originalQueueName = "my_queue";
            var deadLetterExchangeName = "my_dead_letter_exchange";
            var deadLetterQueueName = "my_dead_letter_queue";
            var routingKey = "my_routing_key";
            var message = "Hello, RabbitMQ!";

            var producer = new EventProducer(rabbitMqConnectionString, exchangeName, originalQueueName,
                deadLetterExchangeName, deadLetterQueueName, routingKey);
            producer.PublishEvent(message);
        }

    }
}
