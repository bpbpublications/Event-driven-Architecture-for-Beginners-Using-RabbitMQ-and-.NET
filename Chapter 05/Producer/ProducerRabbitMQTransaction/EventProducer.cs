using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Security.Policy;
using System.Text;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using System.Threading.Channels;

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
            channel.TxSelect();

            try
            {
                // Publish message
               
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: null, body: body);

                channel.TxCommit();

                Console.WriteLine("Message published successfully!");
            }
            catch (Exception ex)
            {
                channel.TxRollback();
                Console.WriteLine("Failed to publish the message: " + ex.Message);
            }
        }
      

    }
}
    

