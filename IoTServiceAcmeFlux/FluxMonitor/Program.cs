using CommandLine;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.Flux.Monitor
{
    class Program
    {
        #region Fields
        static string eventHubName;
        static int numberOfMessages;
        static string activityType;
        static string readerConnectionString = string.Empty;
        static string senderConnectionString = string.Empty;
        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("Starting. . .");

            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                eventHubName = options.eventHubName;
                numberOfMessages = options.messageCount;
                activityType = options.activityType;
            }

            if (activityType == "Receive")
            {
                readerConnectionString = GetServiceBusConnectionString("Receive");
                Receiver r = new Receiver(eventHubName, readerConnectionString);
                r.MessageProcessingWithPartitionDistribution();
            }

            if (activityType == "Send")
            {
                senderConnectionString = GetServiceBusConnectionString("Send");
                Sender s = new Sender(senderConnectionString, numberOfMessages,eventHubName);
                s.SendEvents();            
            }

            Console.WriteLine("Press enter key to stop worker.");
            Console.ReadLine();
        }

        private static string GetServiceBusConnectionString(string activityType)
        {
            string appSettingsKey = string.Format("ServiceBus{0}", activityType);
            string connectionString = ConfigurationManager.AppSettings[appSettingsKey];
            
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Did not find Service Bus connections string in appsettings (app.config)");
                return string.Empty;
            }
            ServiceBusConnectionStringBuilder builder = new ServiceBusConnectionStringBuilder(connectionString);
            builder.TransportType = TransportType.Amqp;
            return builder.ToString();
        }
    }
}
