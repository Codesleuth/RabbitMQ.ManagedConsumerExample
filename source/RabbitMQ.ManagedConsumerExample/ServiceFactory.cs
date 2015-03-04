using RabbitMQ.Client;
using RabbitMQ.ManagedConsumerExample.queueing;

namespace RabbitMQ.ManagedConsumerExample
{
    public static class ServiceFactory
    {
        private static readonly IConnectionFactory _connectionFactory;

        static ServiceFactory()
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

        public static Server CreateServer()
        {
            var managedConsumer = new CustomConsumer();

            return new Server(CreateConnectionFactory(), managedConsumer, (connectionFactory, consumer) => new QueueWrapper(connectionFactory, consumer));
        }

        private static IConnectionFactory CreateConnectionFactory()
        {
            return _connectionFactory;
        }
    }
}