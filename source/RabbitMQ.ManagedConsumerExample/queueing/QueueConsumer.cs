using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.ManagedConsumerExample.interfaces;
using RabbitMQ.ManagedConsumerExample.models;

namespace RabbitMQ.ManagedConsumerExample.queueing
{
    public class QueueConsumer
    {
        private readonly IModel _channel;
        private readonly Func<QueueingBasicConsumer> _consumerFactory;
        private readonly IManagedConsumer _managedConsumer;
        private QueueingBasicConsumer _queueingBasicConsumer;
        private CancellationTokenSource _cancellationToken;
        private string _consumerName;

        public QueueConsumer(IModel channel, Func<QueueingBasicConsumer> consumerFactory, IManagedConsumer managedConsumer)
        {
            _channel = channel;
            _consumerFactory = consumerFactory;
            _managedConsumer = managedConsumer;
        }

        private void BeginConsumption()
        {
            _queueingBasicConsumer = _consumerFactory();
            _consumerName = _channel.BasicConsume("MyQueue", false, _queueingBasicConsumer);
        }

        private void CancelConsumption()
        {
            _channel.BasicCancel(_consumerName);
        }

        public void Start()
        {
            _cancellationToken = new CancellationTokenSource();

            BeginConsumption();

            Task.Factory.StartNew(ProcessQueue);
        }

        private void ProcessQueue()
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
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        Console.WriteLine("{0:o} :: Cancellation requested, exiting consumption loop", DateTime.Now);
                        return;
                    }

                    Console.WriteLine("{0:o} :: Consumer failed with EndOfStreamException, reconsuming...", DateTime.Now);

                    CancelConsumption();
                    BeginConsumption();
                }

                if (dequeued)
                {
                    ProcessResult result = null;
                    try
                    {
                        result = _managedConsumer.ProcessMessage(args);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Managed consumer threw exception: {0}", ex.Message);
                        SendNotAcknowedged(args.DeliveryTag, false);
                    }

                    if (result != null)
                    {
                        switch (result.Status)
                        {
                            case ResultStatus.Acknowledged:
                                SendAcknowledged(args.DeliveryTag);
                                break;
                            default:
                                SendNotAcknowedged(args.DeliveryTag);
                                break;
                        }
                    }
                }

                Task.Factory.StartNew(ProcessQueue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception! {0}", ex.Message);
                throw;
            }
        }

        private void SendAcknowledged(ulong deliveryTag)
        {
            _channel.BasicAck(deliveryTag, false);
        }

        private void SendNotAcknowedged(ulong deliveryTag, bool requeue = true)
        {
            _channel.BasicNack(deliveryTag, false, requeue);
        }

        public void Stop()
        {
            _cancellationToken.Cancel();
            CancelConsumption();
        }
    }
}