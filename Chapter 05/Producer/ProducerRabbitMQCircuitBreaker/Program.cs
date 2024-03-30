namespace ProducerRabbitMQ
{
    class Program
    {
        static void Main(string[] args)
        {
            var producer = new RabbitMQProducer();
            producer.PublishMessage("Hello, RabbitMQ!");

        }

    }
}
