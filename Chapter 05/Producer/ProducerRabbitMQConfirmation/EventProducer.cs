using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

public class EventProducer
{
    private readonly string rabbitMqConnectionString;
    private readonly string exchangeName;

    public EventProducer(string rabbitMqConnectionString, string exchangeName)
    {
        this.rabbitMqConnectionString = rabbitMqConnectionString;
        this.exchangeName = exchangeName;
    }

    public void PublishEvent(string exchange,string routingKey, string message)
    {
        var factory = new ConnectionFactory { Uri = new Uri(rabbitMqConnectionString) };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
           
                channel.ConfirmSelect();

                // Publish message
              
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: null, body: body);

                if (channel.WaitForConfirms())
                {
                    Console.WriteLine("Message published successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to publish the message.");
                }
            }
        }
    
}
 