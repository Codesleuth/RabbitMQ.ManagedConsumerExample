using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.ManagedConsumerExample.interfaces;
using RabbitMQ.ManagedConsumerExample.models;

namespace RabbitMQ.ManagedConsumerExample.queueing
{
    public class QueueWrapper : IQueueWrapper
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IConnectionFactory _connectionFactory;
        private readonly IManagedConsumer _consumer;
        private IConnection _connection;
        private IModel _channel;
        private QueueingBasicConsumer _queueingBasicConsumer;
        private bool _cancel;
        private string _consumerTag;

        public QueueWrapper(IConnectionFactory connectionFactory, IManagedConsumer consumer)
        {
            _connectionFactory = connectionFactory;
            _consumer = consumer;
        }

        public void Start()
        {
            Task.Factory.StartNew(ConnectQueue);
        }

        private void ConnectQueue()
        {
            try
            {
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();

                DeclareAndBind();
                CreateConsumer();
            }
            catch (Exception)
            {
                _log.Error("Unable to connect queue and consume. Retrying...");
                Thread.Sleep(500);
                Task.Factory.StartNew(ConnectQueue);
                return;
            }

            _log.Info("Processing consumer...");
            Task.Factory.StartNew(Consume);
        }

        private void DeclareAndBind()
        {
            _channel.ExchangeDeclare("MyExchange", "headers", false, true, null);
            _channel.QueueDeclare("MyQueue", false, false, true, null);
            _channel.QueueBind("MyQueue", "MyExchange", string.Empty);
        }

        private void CreateConsumer()
        {
            _queueingBasicConsumer = new QueueingBasicConsumer(_channel);
            _queueingBasicConsumer.ConsumerCancelled += (sender, args) => _log.Error("ConsumerCancelled event");
            _consumerTag = _channel.BasicConsume("MyQueue", false, _queueingBasicConsumer);

            _log.Info("Consumer attached to channel.");
        }

        private void Consume()
        {
            if (_cancel)
            {
                if (_queueingBasicConsumer.IsRunning)
                    _channel.BasicCancel(_queueingBasicConsumer.ConsumerTag);

                _channel.Close();
                _connection.Close();
                return;
            }

            try
            {
                BasicDeliverEventArgs args;
                var dequeued = _queueingBasicConsumer.Queue.Dequeue(5000, out args);
                if (dequeued)
                {
                    _log.InfoFormat("Message was dequeued: {0}", args);

                    try
                    {
                        var processResult = _consumer.ProcessMessage(args);

                        switch (processResult.Status)
                        {
                            case ResultStatus.Acknowledged:
                                _channel.BasicAck(args.DeliveryTag, false);
                                break;
                            case ResultStatus.NotAcknowledged:
                                _channel.BasicNack(args.DeliveryTag, false, false);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Fatal("Unable to process message", ex);
                        _channel.BasicNack(args.DeliveryTag, false, true);
                        _cancel = true;
                    }
                }
            }
            catch (EndOfStreamException endOfStreamException)
            {
                _log.Error("Unable to dequeue", endOfStreamException);

                if (_channel.IsOpen)
                {
                    _channel.BasicCancel(_consumerTag);
                    DeclareAndBind();

                    try
                    {
                        CreateConsumer();

                        _log.Info("Consumer has recovered");
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Unable to recover consumption", ex);
                    }
                }

                Thread.Sleep(500);
            }

            Task.Factory.StartNew(Consume);
        }

        public void Stop()
        {
            _cancel = true;
            _log.Info("Waiting for consumer to end...");
        }
    }
}