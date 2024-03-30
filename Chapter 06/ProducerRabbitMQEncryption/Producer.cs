using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SecureEventProducer
{
    public class Producer
    {
        private readonly string _queueName;
        private readonly string _rabbitMQConnectionString;
        private readonly byte[] _encryptionKey;

        public Producer(string queueName, string rabbitMQConnectionString, byte[] encryptionKey)
        {
            _queueName = queueName;
            _rabbitMQConnectionString = rabbitMQConnectionString;
            _encryptionKey = encryptionKey;
        }

        public void PublishSensitiveEvent(string sensitiveMessage)
        {
            try
            {
                var encryptedMessage = EncryptMessage(sensitiveMessage, _encryptionKey);

                var factory = new ConnectionFactory() { Uri = new Uri(_rabbitMQConnectionString) };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);

                    var body = Encoding.UTF8.GetBytes(encryptedMessage);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true; // Make the message persistent

                    channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: properties, body: body);

                    Console.WriteLine(" [x] Sent sensitive message: " + sensitiveMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error publishing sensitive event: " + ex.Message);
            }
        }

        private string EncryptMessage(string message, byte[] key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.GenerateIV();

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                byte[] encrypted;

                using (var msEncrypt = new System.IO.MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(message);
                    }
                    encrypted = msEncrypt.ToArray();
                }

                byte[] ivAndEncrypted = new byte[aesAlg.IV.Length + encrypted.Length];
                Array.Copy(aesAlg.IV, ivAndEncrypted, aesAlg.IV.Length);
                Array.Copy(encrypted, 0, ivAndEncrypted, aesAlg.IV.Length, encrypted.Length);

                return Convert.ToBase64String(ivAndEncrypted);
            }
        }
    }

    public class Consumer
    {
        private readonly string _queueName;
        private readonly string _rabbitMQConnectionString;
        private readonly byte[] _encryptionKey;

        public Consumer(string queueName, string rabbitMQConnectionString, byte[] encryptionKey)
        {
            _queueName = queueName;
            _rabbitMQConnectionString = rabbitMQConnectionString;
            _encryptionKey = encryptionKey;
        }

        public void StartConsuming()
        {
            var factory = new ConnectionFactory() { Uri = new Uri(_rabbitMQConnectionString) };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var encryptedMessage = Encoding.UTF8.GetString(body);

                        var decryptedMessage = DecryptMessage(encryptedMessage, _encryptionKey);

                        Console.WriteLine(" [x] Received sensitive message: " + decryptedMessage);

                        channel.BasicAck(ea.DeliveryTag, multiple: false); // Acknowledge message
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing message: " + ex.Message);
                        channel.BasicReject(ea.DeliveryTag, requeue: false);
                    }
                };

                channel.BasicConsume(_queueName, autoAck: false, consumer);
                Console.WriteLine("Consumer started. Waiting for messages...");
                Console.ReadLine(); // Keep the application running until manually stopped.
            }
        }

        private string DecryptMessage(string encryptedMessage, byte[] key)
        {
            byte[] ivAndEncrypted = Convert.FromBase64String(encryptedMessage);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = ivAndEncrypted.Take(aesAlg.IV.Length).ToArray();

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                string decryptedMessage;

                using (var msDecrypt = new System.IO.MemoryStream(ivAndEncrypted.Skip(aesAlg.IV.Length).ToArray()))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                {
                    decryptedMessage = srDecrypt.ReadToEnd();
                }

                return decryptedMessage;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string queueName = "my_event_queue";
            string rabbitMQConnectionString = "amqp://guest:guest@localhost:5672/";
            byte[] encryptionKey = Encoding.UTF8.GetBytes("0123456789ABCDEF"); // Replace this with a strong encryption key.

            var producer = new Producer(queueName, rabbitMQConnectionString, encryptionKey);
            producer.PublishSensitiveEvent("This is a sensitive message.");

            var consumer = new Consumer(queueName, rabbitMQConnectionString, encryptionKey);
            consumer.StartConsuming();
        }
    }
}
