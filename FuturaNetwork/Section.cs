using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UndoRedoFramework;

namespace FuturaNetwork
{
    [Serializable]
    public class Section : INetworkObject
    {
        public UndoRedo<int> objectIdUR = new UndoRedo<int>();
        public UndoRedo<double> xUR = new UndoRedo<double>();
        public UndoRedo<double> yUR = new UndoRedo<double>();
        public UndoRedo<double> toxUR = new UndoRedo<double>();
        public UndoRedo<double> toyUR = new UndoRedo<double>();
        public UndoRedo<int> oidUR = new UndoRedo<int>();
        public UndoRedo<string> uidUR = new UndoRedo<string>();
        public UndoRedo<int> statusUR = new UndoRedo<int>();
        public UndoRedo<int> phaseCodeUR = new UndoRedo<int>();
        public UndoRedo<int> constructedPhaseCodeUR = new UndoRedo<int>();
        public UndoRedo<Node> parentNodeUR = new UndoRedo<Node>();
        public UndoRedo<Node> childNodeUR = new UndoRedo<Node>();
        public UndoRedo<int> classIDUR = new UndoRedo<int>();
        public string uid
        {
            get
            {
                return uidUR.Value;
            }
            set
            {
                uidUR.Value = value;
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

        public double tox
        {
            get
            {
                return toxUR.Value;
            }
            set
            {
                toxUR.Value = value;
            }
        }

        public double toy
        {
            get
            {
                return toyUR.Value;
            }
            set
            {
                toyUR.Value = value;
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

        public Node parentNode
        {
            get
            {
                return parentNodeUR.Value;
            }
            set
            {
                parentNodeUR.Value = value;
            }
        }

        public Node childNode
        {
            get
            {
                return childNodeUR.Value;
            }
            set
            {
                childNodeUR.Value = value;
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





        public Section()
        {
        }
        //public Section(Node pn, Node cn, string ui, int stats, int pc, int Session)
        //{
        //    ParentNode = pn;
        //    ChildNode = cn;
        //    uid = ui;
        //    status = stats;
        //    phaseCode = pc;
        //}
        //public Section(Node pn, Node cn, int objid, int oidPassed, string ui, int stats, int pc)
        //{
        //    ParentNode = pn;
        //    ChildNode = cn;
        //    objectId = objid;
        //    Oid = oidPassed;
        //    uid = ui;
        //    status = stats;
        //    phaseCode = pc;
        //}
        //public Section(Node pn, Node cn, int X, int Y, int objid, int oidPassed, string ui, int stats, int pc)
        //{
        //    x = X;
        //    y = Y;
        //    ParentNode = pn;
        //    ChildNode = cn;
        //    objectId = objid;
        //    Oid = oidPassed;
        //    uid = ui;
        //    status = stats;
        //    phaseCode = pc;
        //}
        //public static bool operator ==(Node x, Node y)
        //{
        //    return x.uid == y.uid && x.status == y.status && x.phaseCode == y.phaseCode && x.SessionId == y.SessionId;
        //}
        //public static bool operator !=(Node x, Node y)
        //{
        //    return !(x == y);
        //}
    }



}
