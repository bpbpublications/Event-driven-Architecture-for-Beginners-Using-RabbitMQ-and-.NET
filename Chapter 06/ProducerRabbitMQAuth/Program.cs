using System;

namespace RabbitMQAuthenticationExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create an instance of the event producer
            var eventProducer = new EventProducer();

            try
            {
                // Publish the message
                eventProducer.PublishMessage("Hello RabbitMQ");
                Console.WriteLine("Message published successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error publishing message: " + ex.Message);
            }

            Console.ReadLine();
        }
    }
}
