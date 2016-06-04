using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuturaNetwork
{
    public interface INetworkObject
    {

        string uid { get; set; }
        int oid { get; set; }
        int classID { get; set; }
        int status { get; set; }
        int phaseCode { get; set; }
    }
}
