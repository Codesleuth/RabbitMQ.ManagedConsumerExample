using RabbitMQ.Client.Events;
using RabbitMQQueueingBasicConsumer.models;

namespace RabbitMQQueueingBasicConsumer.interfaces
{
    public interface IManagedConsumer
    {
        ProcessResult ProcessMessage(BasicDeliverEventArgs args);
    }
}