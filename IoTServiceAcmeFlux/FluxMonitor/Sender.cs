using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acme.Flux.Monitor.Models;

namespace Acme.Flux.Monitor
{
    public class Sender
    {
        const int numberOfDevices = 1000;
        string _connectionString = string.Empty;
        string _eventHubName = string.Empty;
        int numberOfMessages;
        public Sender(string connectionString, int numberOfMessages, string eventHubName)
        {
            this._connectionString = connectionString;
            this.numberOfMessages = numberOfMessages;
            this._eventHubName = eventHubName;
        }

        /// <summary>
        /// Creates sample messages and sends to the Event Hub
        /// </summary>
        /// <remarks>
        /// The construction of both message types within the loop was done to simplify the code in lieu of flexibility
        /// and ease of maintenance. A cleaner approach would to be to refactor out the message construction.
        /// </remarks>
        public void SendEvents()
        {
            // Create EventHubClient
            EventHubClient client = EventHubClient.CreateFromConnectionString(_connectionString,_eventHubName);

            try
            {
                List<Task> tasks = new List<Task>();
                Random random = new Random();

                Console.WriteLine("Sending messages to Event Hub {0}", client.Path);

                for(int i = 0 ; i < this.numberOfMessages; ++i)
                {
                    int deviceIdValue = random.Next(numberOfDevices);

                    // Create a FluxMakerStatus message //
                    FluxMakerStatus makerStatus = new FluxMakerStatus()
                    {
                        DeviceId = deviceIdValue,
                        Temperature = random.Next(100),
                        Timestamp = DateTime.UtcNow.ToString(),
                        Humdity = random.Next(68),
                        Operation = GetOperation(deviceIdValue)
                    };

                    var makerStatusSerialized = JsonConvert.SerializeObject(makerStatus);
                    EventData makerStatusData = new EventData(Encoding.UTF8.GetBytes(makerStatusSerialized)) 
                                        {
                                            PartitionKey = makerStatus.DeviceId.ToString()
                                        };

                    // Set optional user properties
                    makerStatusData.Properties.Add("Type","Telemetry_" + DateTime.UtcNow.ToLongTimeString());
                    OutputFluxMakerMessage("SENDING: ", makerStatusData, makerStatus);

                    // Add the sending of FluxMakerStatus to Event Hub to the task list
                    tasks.Add(client.SendAsync(makerStatusData));


                    // Create a Flux QA message //
                    FluxQAStatus qaStatus = new FluxQAStatus()
                    {
                        DeviceId = deviceIdValue,
                        Timestamp = DateTime.UtcNow.ToString(),
                        LastFluxCapacitorType = GetFluxType(deviceIdValue)
                    };

                    var qaStatusSerialized = JsonConvert.SerializeObject(qaStatus);
                    EventData qaStatusData = new EventData(Encoding.UTF8.GetBytes(qaStatusSerialized)) 
                                        {
                                            PartitionKey = qaStatus.DeviceId.ToString()
                                        };

                    // Set optional user properties
                    qaStatusData.Properties.Add("Type","Telemetry_" + DateTime.UtcNow.ToLongTimeString());
                    OutputFluxQAMessage("SENDING: ", qaStatusData, qaStatus);

                    // Add the sending of FluxQAStatus to Event Hub to the task list
                    tasks.Add(client.SendAsync(qaStatusData));
                    Console.WriteLine();
                };

                // Wait on the list of tasks to be executed.
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception exp)
            {
                Console.WriteLine("Error on send: " + exp.Message);
            }
        }

        #region Helper Methods

        /// <summary>
        /// Determines the flux manufacturing method in psuedo-random fashion.
        /// </summary>
        private static string GetOperation(int deviceId)
        {
            string op = string.Empty;
            int modulomath = deviceId & 25;


            if (modulomath > 20)
            {
                op = "stamp";
            }
            else if (modulomath > 10)
            {
                op = "load";
            }
            else if (modulomath < 10)
            {
                op = "switch";
            }

            return op;
        }

        /// <summary>
        /// Determines the flux capacitor color in pseudo-random fashion.
        /// </summary>
        private static string GetFluxType(int deviceId)
        {
            string fluxType = string.Empty;
            int modulomath = deviceId % 12;

            if (modulomath <=4)
            {
                fluxType = "red";
            }
            else if ((modulomath > 4) && (modulomath < 10))
            {
                fluxType = "blue";
            }
            else if (modulomath >=10)
            {
                fluxType = "green";
            }

            return fluxType;
        }


        /// <summary>
        /// Writes out a message containing the parameters of the FluxMakerStatus message being sent to the Event Hub
        /// </summary>
        static void OutputFluxMakerMessage(string action, EventData data, FluxMakerStatus status)
        {
            if (data == null)
            {
                return;
            }
            if (status != null)
            {
                Console.WriteLine("{0}{1} - Device {2}, Temperature {3}, Timestamp {4}, Humidity {5}, Operation {6}"
                    , action, status, status.DeviceId, status.Temperature, status.Timestamp, status.Humdity, status.Operation);
            }
        }

        /// <summary>
        /// Writes out a message containing the parameters of the FluxQAStatus message being sent to the Event Hub
        /// </summary>
        static void OutputFluxQAMessage(string action, EventData data, FluxQAStatus status)
        {
            if (data == null)
            {
                return;
            }
            if (status != null)
            {
                Console.WriteLine("{0}{1} - Device {2}, Timestamp {3}, LastFluxCapacitoryType {4}", action, status, status.DeviceId, status.Timestamp, status.LastFluxCapacitorType);
            }
        }
        #endregion
    }
}
