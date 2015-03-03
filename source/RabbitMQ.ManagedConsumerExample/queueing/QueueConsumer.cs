using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.ManagedConsumerExample.interfaces;
using RabbitMQ.ManagedConsumerExample.models;
using MethodBase = System.Reflection.MethodBase;

namespace RabbitMQ.ManagedConsumerExample.queueing
{
    public class QueueConsumer
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
                    _log.Info("Cancellation requested, exiting consumption loop");
                    return;
                }

                _log.Info("Consumer failed with EndOfStreamException");

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
                    _log.Error("Managed consumer threw exception: {0}", ex);
                    SendNotAcknowedged(args.DeliveryTag, false);
                }

                if (result != null)
                {
                    if (result.Status == ResultStatus.Acknowledged)
                    {
                        SendAcknowledged(args.DeliveryTag);
                        if (result.PauseMilliseconds.HasValue)
                        {
                            Thread.Sleep(result.PauseMilliseconds.Value);
                            _log.Info("Resuming");
                        }
                    }
                    else
                    {
                        SendNotAcknowedged(args.DeliveryTag);
                    }
                }
            }

            Task.Factory.StartNew(ProcessQueue);
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