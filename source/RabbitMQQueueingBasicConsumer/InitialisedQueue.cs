using RabbitMQ.Client;

namespace RabbitMQQueueingBasicConsumer
{
    public class InitialisedQueue
    {
        public IConnection Connection { get; private set; }
        public IModel Channel { get; private set; }

        public InitialisedQueue(IConnection connection, IModel channel)
        {
            Connection = connection;
            Channel = channel;
        }
    }
}