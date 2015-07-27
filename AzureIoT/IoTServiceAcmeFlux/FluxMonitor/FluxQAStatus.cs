using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Acme.Flux.Monitor.Models
{
    /// <summary>
    /// Class presenting the latest flux capacitor that went through our QA checkpoint machine.
    /// </summary>
    [DataContract]
    public class FluxQAStatus
    {
        [DataMember]
        public int DeviceId { get; set; }

        [DataMember]
        public string Timestamp {get;set;}

        [DataMember]
        public string LastFluxCapacitorType { get; set; }

    }
}
