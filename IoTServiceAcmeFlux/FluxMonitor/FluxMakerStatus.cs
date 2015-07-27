using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Acme.Flux.Monitor.Models
{
    /// <summary>
    /// Class representing the status items reported by a flux capacitor maker machine.
    /// </summary>
    [DataContract]
    public class FluxMakerStatus
    {
        [DataMember]
        public int DeviceId { get; set; }

        [DataMember]
        public string Timestamp {get;set;}

        [DataMember]
        public string Operation { get; set; }

        [DataMember]
        public int Temperature { get; set; }

        [DataMember]
        public int Humdity { get; set; }

    }
}
