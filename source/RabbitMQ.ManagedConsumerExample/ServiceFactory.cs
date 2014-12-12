using RabbitMQ.Client;
using RabbitMQ.ManagedConsumerExample.queueing;

namespace RabbitMQ.ManagedConsumerExample
{
    public static class ServiceFactory
    {
        public static Service CreateServer()
        {
            var queueInitialiser = new QueueInitialiser();
            var customConsumer = new CustomConsumer();

            return new Service(queueInitialiser, customConsumer, (channel, consumer) => new QueueConsumer(channel, () => new QueueingBasicConsumer(channel), consumer));
        }
    }
}