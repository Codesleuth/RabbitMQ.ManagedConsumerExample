using RabbitMQ.Client;
using RabbitMQ.ManagedConsumerExample.models;

namespace RabbitMQ.ManagedConsumerExample.queueing
{
    public class QueueInitialiser
    {
        private static readonly ConnectionFactory _connectionFactory;

        static QueueInitialiser()
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                AutomaticRecoveryEnabled = true,
                RequestedHeartbeat = 15
            };
        }

        public InitialisedQueue InitialiseQueue()
        {
            var connection = _connectionFactory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare("MyQueue", true, false, false, null);
            channel.QueueBind("MyQueue", "MyExchange", string.Empty);

            return new InitialisedQueue(connection, channel);
        }
    }
}