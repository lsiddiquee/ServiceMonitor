using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ServiceMonitor.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Configuration config = new Configuration(args);
                SubscribeToLogFileChanges(config);
                SubscribeToEventLogs(config);
                WaitForServicesToStop(config);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                throw;
            }
        }

        private static Tail[] fileTails;

        private static void SubscribeToLogFileChanges(Configuration config)
        {
            if (!config.LogFiles.Any())
                return;

            fileTails = new Tail[config.LogFiles.Count];
            int index = 0;
            foreach (string configLogFile in config.LogFiles)
            {
                System.Console.WriteLine($"Starting to tail {configLogFile}.");
                fileTails[index] = new Tail(configLogFile);
                fileTails[index].FileChanged += (sender, args) => { System.Console.WriteLine($"{sender}: {args}"); };
                index++;
            }
        }

        private static EventLog[] eventLogs;
        private static void SubscribeToEventLogs(Configuration config)
        {
            if (!config.EventLogs.Any())
                return;

            eventLogs = new EventLog[config.EventLogs.Count];
            int index = 0;
            foreach (string eventLog in config.EventLogs)
            {
                System.Console.WriteLine($"Starting to monitor {eventLog} windows event log.");
                eventLogs[index] = new EventLog(eventLog);
                eventLogs[index].EntryWritten += (sender, args) => { System.Console.WriteLine(args.Entry.Message); }; 
                eventLogs[index].EnableRaisingEvents = true;
            }
        }

        private static void WaitForServicesToStop(Configuration config)
        {
            if (!config.Services.Any())
            {
                System.Console.WriteLine("No services mentioned to monitor, hence closing application.");
                return;
            }

            ManualResetEvent waitHandle = new ManualResetEvent(false);

            ServiceMonitor.ServiceStopped += (sender, s) =>
            {
                System.Console.WriteLine($"Service stopped {s}");
                waitHandle.Set();
            };
            foreach (string service in config.Services)
            {
                System.Console.WriteLine($"Starting to monitor {service} windows service.");
                ServiceMonitor.StartMonitor(service);
            }

            waitHandle.WaitOne();
        }
    }

    class Configuration
    {
        public ICollection<string> Services { get; private set; }

        public ICollection<string> EventLogs { get; private set; }

        public ICollection<string> LogFiles { get; private set; }

        public Configuration(string[] args)
        {
            List<string> services = new List<string>();
            List<string> eventLogs = new List<string>();
            List<string> logFiles = new List<string>();

            List<string> container = null;

            foreach (string arg in args)
            {
                switch (arg.ToLowerInvariant())
                {
                    case "-s":
                        container = services;
                        break;
                    case "-e":
                        container = eventLogs;
                        break;
                    case "-l":
                        container = logFiles;
                        break;
                    default:
                        container?.Add(arg);
                        break;
                }
            }

            Services = services;
            EventLogs = eventLogs;
            LogFiles = logFiles;
        }

        public static string GetUsage()
        {
            return $"'{Environment.CommandLine}' [-s] [-e] [-l]";
        }
    }
}
