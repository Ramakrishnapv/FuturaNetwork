using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FuturaNetwork;
using ESRI.ArcGIS.Geometry;

namespace Futura.ArcGIS.NetworkExtension
{
    public class EditObject
    {
        public EditType Type { get; set; }
        public string ClassName { get; set; }
        public int CLSID { get; set; }
        public int OID { get; set; }
        public string UID { get; set; }
        public ElementType ElementType { get; set; }
        public ConnectivityInfo Connectivity { get; set; }
        public EditObject Partner { get; set; } //only incase of split & merge
        public bool IsNetworkProtector { get; set; }
        public bool IsSource { get; set; }
        public bool IsYDTransformer { get; set; }
        public IGeometry shape { get; set; }
    }

    //public class EditObjConnectivity
    //{
    //    public INetworkObject Parent { get; set; }
    //    public int PhaseCode { get; set; }
    //    public string ConstructedPhase { get; set; }
    //    public List<FuturaNetwork.Node> SnappedNodes { get; set; } //both snapped nodes at from pt & to point for line
    //    public FuturaNetwork.Node ParentNode { get; set; } //only for section
    //    public FuturaNetwork.Node ChildNode { get; set; } // only for section
    //    public List<FuturaNetwork.Section> ConnectedEdges { get; set; }
    //    public bool SetFlow { get; set; } // only for section
    //    public int Status { get; set; }// only for section
    //}

    public class ParentFeature
    {
        public int CLSID { get; set; }
        public int OID { get; set; }
        public string UID { get; set; }
    }

    public enum EditType
    {
        Add,
        FieldUpdate,
        ShapeUpdate,
        Delete,
        Split,
        Merge,
        Move
    }

    public enum ElementType
    {
        Node,
        Section,
        None
    }
}
