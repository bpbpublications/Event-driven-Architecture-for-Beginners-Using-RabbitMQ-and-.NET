namespace ProducerRabbitMQ
{
    class Program
    {
        static void Main(string[] args)
        {
            var rabbitMqConnectionString = "amqp://guest:guest@localhost:";
            var producer = new EventProducer(rabbitMqConnectionString);

            var exchangeName = "my_exchange";
            var routingKey = "my_event";
            var message = "Hello, RabbitMQ!";

            producer.PublishEvent(exchangeName, routingKey, message);
        }
    }
}
