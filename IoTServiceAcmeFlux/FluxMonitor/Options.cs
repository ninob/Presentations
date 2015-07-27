using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Acme.Flux.Monitor
{
    /// <summary>
    /// Defines command-line argument options. Required by the CommandLine parser library.
    /// </summary>
    class Options
    {
        [Option('e', "eventHubName", HelpText = "the name of the event hub")]
        public string eventHubName { get; set; }

        [Option('m', "messageCount", DefaultValue = 100, HelpText = "The number of messages to send to the event hub")]
        public int messageCount { get; set; }

        [Option('a', "activityType", HelpText = "Indicates if this is receiving or sending messages from/to the Event Hub. Receive or Send are only accepted values")]
        public string activityType { get; set; }
    }
}
