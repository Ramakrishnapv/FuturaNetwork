using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuturaNetwork
{
    public class ConnectivityInfo
    {
        public INetworkObject parentFeature;
        public List<Node> snappedNodes;
        public int phaseCode;
        public int constructedPhaseCode;
        public List<Section> connectedEdges;
        public Node parentNode;
        public Node childNode;
        public int status;
        public bool setFlow;
    }
}
