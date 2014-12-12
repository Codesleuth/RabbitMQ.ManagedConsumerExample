using System;
using RabbitMQ.Client;
using RabbitMQ.ManagedConsumerExample.interfaces;
using RabbitMQ.ManagedConsumerExample.models;
using RabbitMQ.ManagedConsumerExample.queueing;

namespace RabbitMQ.ManagedConsumerExample
{
    public class Service
    {
        private readonly QueueInitialiser _queueInitialiser;
        private readonly IManagedConsumer _managedConsumer;
        private readonly Func<IModel, IManagedConsumer, QueueConsumer> _queueConsumerFactory;
        private InitialisedQueue _initialisedQueue;
        private QueueConsumer _queueConsumer;

        public Service(QueueInitialiser queueInitialiser, IManagedConsumer managedConsumer, Func<IModel, IManagedConsumer, QueueConsumer> queueConsumerFactory)
        {
            _queueInitialiser = queueInitialiser;
            _managedConsumer = managedConsumer;
            _queueConsumerFactory = queueConsumerFactory;
        }

        public void Start()
        {
            _initialisedQueue = _queueInitialiser.InitialiseQueue();
            _queueConsumer = _queueConsumerFactory(_initialisedQueue.Channel, _managedConsumer);
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