using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UndoRedoFramework.Collections.Generic;
using UndoRedoFramework;

namespace FuturaNetwork
{
    [Serializable]
    public class Node : INetworkObject
    {
        public UndoRedo<int> objectIdUR = new UndoRedo<int>();
        public UndoRedo<double> xUR = new UndoRedo<double>();
        public UndoRedo<double> yUR = new UndoRedo<double>();
        public UndoRedo<int> oidUR = new UndoRedo<int>();
        public UndoRedo<string> uidUR = new UndoRedo<string>();
        public UndoRedo<bool> isAdjUR = new UndoRedo<bool>();
        public UndoRedo<Node> adjacentNodeUR = new UndoRedo<Node>();
        public UndoRedo<int> statusUR = new UndoRedo<int>();
        public UndoRedo<int> phaseCodeUR = new UndoRedo<int>();
        public UndoRedo<int> constructedPhaseCodeUR = new UndoRedo<int>();
        public UndoRedo<int> classIDUR = new UndoRedo<int>();
        public Dictionary<string, Section> childList = new UndoRedoDictionary<string, Section>();
        public Dictionary<string, Section> parentList = new UndoRedoDictionary<string, Section>();
        public string uid
        {
            get
            {
                return this.uidUR.Value;
            }
            set
            {
                this.uidUR.Value = value;
            }
        }

        public bool isAdj
        {
            get
            {
                return this.isAdjUR.Value;
            }
            set
            {
                this.isAdjUR.Value = value;
            }
        }

        public Node adjacentNode
        {
            get
            {
                return adjacentNodeUR.Value;
            }
            set
            {
                adjacentNodeUR.Value = value;
            }
        }

        public int objectId
        {
            get
            {
                return objectIdUR.Value;
            }
            set
            {
                objectIdUR.Value = value;
            }
        }

        public int classID
        {
            get
            {
                return classIDUR.Value;
            }
            set
            {
                classIDUR.Value = value;
            }
        }

        public double x
        {
            get
            {
                return xUR.Value;
            }
            set
            {
                xUR.Value = value;
            }
        }

        public double y
        {
            get
            {
                return yUR.Value;
            }
            set
            {
                yUR.Value = value;
            }
        }

        public int oid
        {
            get
            {
                return oidUR.Value;
            }
            set
            {
                oidUR.Value = value;
            }
        }



        public int status
        {
            get
            {
                return statusUR.Value;
            }
            set
            {
                statusUR.Value = value;
            }
        }

        public int phaseCode
        {
            get
            {
                return phaseCodeUR.Value;
            }
            set
            {
                phaseCodeUR.Value = value;
            }
        }

        public int constructedPhaseCode
        {
            get
            {
                return constructedPhaseCodeUR.Value;
            }
            set
            {
                constructedPhaseCodeUR.Value = value;
            }
        }

        public Node()
        {
        }
        public Node(string ui, int stats, int pc)
        {
            uid = ui;
            status = stats;
            phaseCode = pc;
        }
        //public Node(int objid, int oidPassed, string ui, int stats, int pc)
        //{
        //    objectId = objid;
        //    oid = oidPassed;
        //    uid = ui;
        //    status = stats;
        //    phaseCode = pc;
        //}
        //public Node(int X, int Y, int objid, int oidPassed, string ui, int stats, int pc)
        //{
        //    x = X;
        //    y = Y;
        //    objectId = objid;
        //    oid = oidPassed;
        //    uid = ui;
        //    status = stats;
        //    phaseCode = pc;
        //}
        //public static bool operator ==(Node x, Node y)
        //{
        //    return x.uid == y.uid && x.status == y.status && x.phaseCode == y.phaseCode && x.SessionId ==y.SessionId;
        //}
        //public static bool operator !=(Node x, Node y)
        //{
        //    return !(x == y);
        //}
    }

}
