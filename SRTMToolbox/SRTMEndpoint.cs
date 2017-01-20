using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRTMToolbox
{
    public class SRTMEndpointCollection : List<SRTMEndpoint>
    {
        public void Add(string name, string location, SRTMEndpointType type = SRTMEndpointType.Web)
        {
            this.Add(new SRTMEndpoint() { Name = name, Location = location, Type = type });
        }
    }


    public class SRTMEndpoint
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public SRTMEndpointType Type { get; set; }

    }

    public enum SRTMEndpointType
    {
        Web,
        Local
    }
}
