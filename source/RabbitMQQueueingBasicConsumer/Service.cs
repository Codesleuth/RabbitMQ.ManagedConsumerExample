using System;
using System.IO;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQQueueingBasicConsumer
{
    public class Service
    {
        private readonly InitialisedQueue _initialiseQueue;
        private QueueingBasicConsumer _queueingBasicConsumer;

        public Service(InitialisedQueue initialiseQueue)
        {
            _initialiseQueue = initialiseQueue;
        }

        private void BeginConsumption()
        {
            _queueingBasicConsumer = new QueueingBasicConsumer();
            _initialiseQueue.Channel.BasicConsume("MyQueue", false, "default", _queueingBasicConsumer);
        }

        private void CancelConsumption()
        {
            _initialiseQueue.Channel.BasicCancel("default");
        }

        public void Start()
        {
            BeginConsumption();

            Task.Factory.StartNew(Go);
        }

        private void Go()
        {
            try
            {

                BasicDeliverEventArgs args = null;
                var dequeued = false;

                try
                {
                    dequeued = _queueingBasicConsumer.Queue.Dequeue(1000, out args);
                }
                catch (EndOfStreamException)
                {
                    CancelConsumption();
                    BeginConsumption();
                }

                if (dequeued)
                {
                    Console.WriteLine("Dequeued a message!");
                    AckMessage(args.DeliveryTag);
                }

                Task.Factory.StartNew(Go);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception! {0}", ex.Message);
                throw;
            }
        }

        private void AckMessage(ulong deliveryTag)
        {
            _initialiseQueue.Channel.BasicNack(deliveryTag, false, false);
        }

        public void Stop()
        {
            CancelConsumption();
            _initialiseQueue.Channel.Close();
            _initialiseQueue.Connection.Close();
        }
    }
}