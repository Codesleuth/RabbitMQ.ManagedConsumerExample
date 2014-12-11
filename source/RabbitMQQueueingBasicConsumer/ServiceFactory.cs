using RabbitMQ.Client;
using RabbitMQQueueingBasicConsumer.queueing;

namespace RabbitMQQueueingBasicConsumer
{
    public static class ServiceFactory
    {
        public static Service CreateServer()
        {
            var queueInitialiser = new QueueInitialiser();
            var customConsumer = new CustomConsumer();

            return new Service(queueInitialiser, channel => new QueueConsumer(channel, () => new QueueingBasicConsumer(channel), customConsumer));
        }
    }
}