using System;

namespace RabbitMQ.ManagedConsumerExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var server = ServiceFactory.CreateServer();
            server.Start();

            Console.WriteLine("Press any key to stop the service...");
            Console.ReadKey();
            Console.WriteLine("Stopping...");

            server.Stop();
        }
    }
}