using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuturaNetwork
{
    public enum NetworkAction
    {
        Add,
        Delete,
        Update,
        None
    }
    public class NetworkOperation
    {
        public NetworkAction action = NetworkAction.None;
        public INetworkObject oldObj;
        public INetworkObject newObj;
    }
}
