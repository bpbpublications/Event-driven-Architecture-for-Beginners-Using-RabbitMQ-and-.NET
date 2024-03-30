static void Main(string[] args)
{
    var rabbitMqConnectionString = "amqp://guest:guest@localhost:5672/";
    var consumer = new EventConsumer(rabbitMqConnectionString);

    var exchangeName = "my_exchange";
    var queueName = "my_queue";
    var routingKey = "my_event";

    consumer.ConsumeEvents(exchangeName, queueName, routingKey);
}
