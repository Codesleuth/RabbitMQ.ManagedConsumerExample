using System;
using System.Text;
using RabbitMQ.Client.Events;
using RabbitMQ.ManagedConsumerExample.interfaces;
using RabbitMQ.ManagedConsumerExample.models;

namespace RabbitMQ.ManagedConsumerExample
{
    public class CustomConsumer : IManagedConsumer
    {
        public ProcessResult ProcessMessage(BasicDeliverEventArgs args)
        {
            var body = Encoding.UTF8.GetString(args.Body);
            Console.WriteLine("{0:o} :: Got message: {1}", DateTime.Now, body);

            if (body == "nack")
            {
                if (!args.Redelivered)
                {
                    Console.WriteLine("{0:o} :: Not Acknowledging message.", DateTime.Now);
                    return new ProcessResult(ResultStatus.NotAcknowledged);
                }
            }

            Console.WriteLine("{0:o} :: Acknowledging message.", DateTime.Now);
            return new ProcessResult(ResultStatus.Acknowledged);
        }
    }
}