using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Timers;


namespace ServiceMonitor.Console
{
    public static class ServiceMonitor
    {
        public static event EventHandler<string> ServiceStopped;

        private static readonly IList<string> services = new List<string>();

        static ServiceMonitor()
        {
            Timer checkForTime = new Timer(5 * 1000); // Run every 5 second
            checkForTime.Elapsed += new ElapsedEventHandler(ValidateServicesRunning);
            checkForTime.Enabled = true;
        }

        private static void ValidateServicesRunning(object sender, ElapsedEventArgs e)
        {
            foreach (string serviceName in services)
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status != ServiceControllerStatus.Running)
                    {
                        ServiceStopped?.Invoke(null, serviceName);
                    }
                }
            }
        }


        public static void StartMonitor(string serviceName)
        {
            services.Add(serviceName);
        }
    }
}
