using System;
using RabbitMQ.Client;
using RabbitMQ.ManagedConsumerExample.models;
using RabbitMQ.ManagedConsumerExample.queueing;

namespace RabbitMQ.ManagedConsumerExample
{
    public class Service
    {
        private readonly QueueInitialiser _queueInitialiser;
        private readonly Func<IModel, QueueConsumer> _queueConsumerFactory;
        private InitialisedQueue _initialisedQueue;
        private QueueConsumer _queueConsumer;

        public Service(QueueInitialiser queueInitialiser, Func<IModel, QueueConsumer> queueConsumerFactory)
        {
            _queueInitialiser = queueInitialiser;
            _queueConsumerFactory = queueConsumerFactory;
        }

        public void Start()
        {
            _initialisedQueue = _queueInitialiser.InitialiseQueue();
            _queueConsumer = _queueConsumerFactory(_initialisedQueue.Channel);
            _queueConsumer.Start();
        }

        public void Stop()
        {
            _queueConsumer.Stop();
            _initialisedQueue.Channel.Close();
            _initialisedQueue.Connection.Close();
        }
    }
}