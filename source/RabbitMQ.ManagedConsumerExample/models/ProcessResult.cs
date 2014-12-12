namespace RabbitMQ.ManagedConsumerExample.models
{
    public class ProcessResult
    {
        public ResultStatus Status { get; private set; }
        public int? PauseMilliseconds { get; private set; }

        public ProcessResult(ResultStatus status)
        {
            Status = status;
        }

        public ProcessResult(ResultStatus status, int pauseMilliseconds)
        {
            Status = status;
            PauseMilliseconds = pauseMilliseconds;
        }
    }
}