using System;
using RabbitMQ.Client;
using RabbitMQ.ManagedConsumerExample.interfaces;

namespace RabbitMQ.ManagedConsumerExample
{
    public class Server
    {
        private readonly IManagedConsumer _managedConsumer;
        private readonly Func<IConnectionFactory, IManagedConsumer, IQueueWrapper> _queueWrapperFactory;
        private readonly IConnectionFactory _connectionFactory;
        private IQueueWrapper _queueWrapper;

        public Server(IConnectionFactory connectionFactory,
                      IManagedConsumer managedConsumer,
                      Func<IConnectionFactory, IManagedConsumer, IQueueWrapper> queueWrapperFactory)
        {
            _connectionFactory = connectionFactory;
            _managedConsumer = managedConsumer;
            _queueWrapperFactory = queueWrapperFactory;
        }

        public void Start()
        {
            _queueWrapper = _queueWrapperFactory(_connectionFactory, _managedConsumer);
            _queueWrapper.Start();
        }

        public void Stop()
        {
            _queueWrapper.Stop();
        }
    }
}