using RabbitMQ.Client.Events;
using RabbitMQ.ManagedConsumerExample.models;

namespace RabbitMQ.ManagedConsumerExample.interfaces
{
    public interface IManagedConsumer
    {
        ProcessResult ProcessMessage(BasicDeliverEventArgs args);
    }
}