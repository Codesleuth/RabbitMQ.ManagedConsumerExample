using System;
using log4net.Config;
using Topshelf;

namespace RabbitMQ.ManagedConsumerExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            HostFactory.Run(hc =>
            {
                hc.UseLog4Net();

                hc.Service<Server>(sc =>
                {
                    sc.ConstructUsing(ServiceFactory.CreateServer);
                    sc.WhenStarted(s => s.Start());
                    sc.WhenStopped(s => s.Stop());
                });

                if (!Environment.UserInteractive)
                    hc.RunAsLocalSystem();

                hc.SetDescription("RabbitMQ Managed Consumer Example.");
                hc.SetServiceName("RabbitMQ.ManagedConsumerExample.Service");
                hc.SetDisplayName("RabbitMQ Managed Consumer Example");

                hc.EnableServiceRecovery(rc =>
                {
                    rc.RestartService(1);
                    rc.SetResetPeriod(0);
                });
            });
        }
    }
}