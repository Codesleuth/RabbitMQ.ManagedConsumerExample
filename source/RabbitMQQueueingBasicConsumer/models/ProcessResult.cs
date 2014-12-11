namespace RabbitMQQueueingBasicConsumer.models
{
    public class ProcessResult
    {
        public ResultStatus Status { get; private set; }

        public ProcessResult(ResultStatus status)
        {
            Status = status;
        }
    }
}