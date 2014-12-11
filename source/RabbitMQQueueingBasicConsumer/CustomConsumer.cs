using System;
using System.Text;
using RabbitMQ.Client.Events;
using RabbitMQQueueingBasicConsumer.interfaces;
using RabbitMQQueueingBasicConsumer.models;

namespace RabbitMQQueueingBasicConsumer
{
    public class CustomConsumer : IManagedConsumer
    {
        public ProcessResult ProcessMessage(BasicDeliverEventArgs args)
        {
            var body = Encoding.UTF8.GetString(args.Body);
            Console.WriteLine("{0:o} :: Got message {1}", DateTime.Now, body);

            if (body == "nack")
            {
                if (!args.Redelivered)
                {
                    Console.WriteLine("{0:o} :: NACK'ing message.", DateTime.Now);
                    return new ProcessResult(ResultStatus.NotAcknowledged);
                }
            }

            return new ProcessResult(ResultStatus.Acknowledged);
        }
    }
}