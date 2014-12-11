using System;
using RabbitMQ.Client;

namespace RabbitMQQueueingBasicConsumer
{
    public class Program
    {
        private static readonly ConnectionFactory ConnectionFactory;

        static Program()
        {
            ConnectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                AutomaticRecoveryEnabled = true,
                RequestedHeartbeat = 15
            };
        }

        public static void Main(string[] args)
        {
            IModel channel;
            var initialisedQueue = InitialiseQueue(out channel);

            var service = new Service(initialisedQueue);
            service.Start();

            Console.WriteLine("Press any key to stop the service...");
            Console.ReadKey();
            Console.WriteLine("Stopping...");

            service.Stop();
        }

        private static InitialisedQueue InitialiseQueue(out IModel channel)
        {
            var connection = ConnectionFactory.CreateConnection();
            channel = connection.CreateModel();

            channel.QueueDeclare("MyQueue", true, false, false, null);
            channel.QueueBind("MyQueue", "MyExchange", string.Empty);

            return new InitialisedQueue(connection, channel);
        }
    }
}
