using System.Reflection;
using System.Text;
using log4net;
using RabbitMQ.Client.Events;
using RabbitMQ.ManagedConsumerExample.interfaces;
using RabbitMQ.ManagedConsumerExample.models;

namespace RabbitMQ.ManagedConsumerExample
{
    public class CustomConsumer : IManagedConsumer
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ProcessResult ProcessMessage(BasicDeliverEventArgs args)
        {
            var body = Encoding.UTF8.GetString(args.Body);
            _log.InfoFormat("Got message: {0}", body);

            if (body == "nack")
            {
                if (!args.Redelivered)
                {
                    _log.Info("Not Acknowledging message.");
                    return new ProcessResult(ResultStatus.NotAcknowledged);
                }
            }
            else if (body == "backoff")
            {
                _log.Info("Backing off for 5 seconds...");
                return new ProcessResult(ResultStatus.Acknowledged, 5000);
            }

            _log.Info("Acknowledging message.");
            return new ProcessResult(ResultStatus.Acknowledged);
        }
    }
}