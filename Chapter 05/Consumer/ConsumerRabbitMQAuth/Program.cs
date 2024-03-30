using System;

namespace RabbitMQAuthenticationExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create an instance of the event consumer
            var eventConsumer = new EventConsumer();

            try
            {
                // Consume events
                eventConsumer.ConsumeEvents();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error consuming events: " + ex.Message);
            }

            Console.ReadLine();
        }
    }
}
