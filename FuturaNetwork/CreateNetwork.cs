using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FuturaNetwork
{
    public class CreateNetwork
    {
        //NetworkUtil netUtil = new NetworkUtil();
        public Dictionary<string, Node> _nodeList = StaticStuff._nodeList;
        public Dictionary<string, Section> _sectionList = StaticStuff._sectionList;
        public DBUtil dbutil = StaticStuff.dbutil;
        public HashSet<string> _sectionGUIDList = new HashSet<string>();
        public HashSet<string> _correctedPCList = new HashSet<string>();
        public HashSet<string> _nodeGUIDList = new HashSet<string>();
        public List<Node> disabledList = new List<Node>();
        public List<Node> phaseErrorList = new List<Node>();
        public List<Node> phaseErrorQueue = new List<Node>();
        public List<Node> phaseCorrectionList = new List<Node>();
        public static NTInfo ntInfo = StaticStuff.ntInfo;
        DataTable dtNetJnList = new DataTable();

        public void AddPointsToPointList(Node dr, Dictionary<Point, List<Node>> pointList)
        {
            int xFloor = Convert.ToInt32(Math.Floor(dr.x));
            int xCeiling = Convert.ToInt32(Math.Ceiling(dr.x));

            int yFloor = Convert.ToInt32(Math.Floor(dr.y));
            int yCeiling = Convert.ToInt32(Math.Ceiling(dr.y));

            //1,2 xfloor
            Point Pt = new Point(xFloor, yFloor);
            if (pointList.ContainsKey(Pt))
            {
                pointList[Pt].Add(dr);
            }
            else
            {
                pointList[Pt] = new List<Node>();
                pointList[Pt].Add(dr);
            }
            Pt = new Point(xFloor, yCeiling);
            if (pointList.ContainsKey(Pt))
            {
                pointList[Pt].Add(dr);
            }
            else
            {
                pointList[Pt] = new List<Node>();
                pointList[Pt].Add(dr);
            }
            //3, 4 xceiling
            Pt = new Point(xCeiling, yFloor);

            if (pointList.ContainsKey(Pt))
            {
                pointList[Pt].Add(dr);
            }
            else
            {
                pointList[Pt] = new List<Node>();
                pointList[Pt].Add(dr);
            }

            Pt = new Point(xCeiling, yCeiling);

            if (pointList.ContainsKey(Pt))
            {
                pointList[Pt].Add(dr);
            }
            else
            {
                pointList[Pt] = new List<Node>();
                pointList[Pt].Add(dr);
            }
        }

        public void AddPointsToLineList(Section dr, Dictionary<Point, List<Section>> lineList)
        {
            int xFloor = Convert.ToInt32(Math.Floor(dr.x));
            int xCeiling = Convert.ToInt32(Math.Ceiling(dr.x));

            int yFloor = Convert.ToInt32(Math.Floor(dr.y));
            int yCeiling = Convert.ToInt32(Math.Ceiling(dr.y));

            int toxFloor = Convert.ToInt32(Math.Floor(dr.tox));
            int toxCeiling = Convert.ToInt32(Math.Ceiling(dr.tox));

            int toyFloor = Convert.ToInt32(Math.Floor(dr.toy));
            int toyCeiling = Convert.ToInt32(Math.Ceiling(dr.toy));

            //add same section as value to 8 key points
            //1,2 xfloor
            Point Pt = new Point(xFloor, yFloor);
            if (lineList.ContainsKey(Pt))
            {
                lineList[Pt].Add(dr);
            }
            else
            {
                lineList[Pt] = new List<Section>();
                lineList[Pt].Add(dr);
            }
            Pt = new Point(xFloor, yCeiling);
            if (lineList.ContainsKey(Pt))
            {
                lineList[Pt].Add(dr);
            }
            else
            {
                lineList[Pt] = new List<Section>();
                lineList[Pt].Add(dr);
            }
            //3, 4 xceiling
            Pt = new Point(xCeiling, yFloor);

            if (lineList.ContainsKey(Pt))
            {
                lineList[Pt].Add(dr);
            }
            else
            {
                lineList[Pt] = new List<Section>();
                lineList[Pt].Add(dr);
            }

            Pt = new Point(xCeiling, yCeiling);

            if (lineList.ContainsKey(Pt))
            {
                lineList[Pt].Add(dr);
            }
            else
            {
                lineList[Pt] = new List<Section>();
                lineList[Pt].Add(dr);
            }

            //5, 6 ToxFloor
            Pt = new Point(toxFloor, toyFloor);

            if (lineList.ContainsKey(Pt))
            {
                lineList[Pt].Add(dr);
            }
            else
            {
                lineList[Pt] = new List<Section>();
                lineList[Pt].Add(dr);
            }
            Pt = new Point(toxFloor, toyCeiling);

            if (lineList.ContainsKey(Pt))
            {
                lineList[Pt].Add(dr);
            }
            else
            {
                lineList[Pt] = new List<Section>();
                lineList[Pt].Add(dr);
            }

            //7, 8 ToxCeiling
            Pt = new Point(toxCeiling, toyFloor);

            if (lineList.ContainsKey(Pt))
            {
                lineList[Pt].Add(dr);
            }
            else
            {
                lineList[Pt] = new List<Section>();
                lineList[Pt].Add(dr);
            }
            Pt = new Point(toxFloor, toyCeiling);

            if (lineList.ContainsKey(Pt))
            {
                lineList[Pt].Add(dr);
            }
            else
            {
                lineList[Pt] = new List<Section>();
                lineList[Pt].Add(dr);
            }
        }

        public bool ContainsAdjNode(Node nd)
        {
            if (nd != null && nd.adjacentNode != null && !string.IsNullOrEmpty(nd.adjacentNode.uid))
                return true;
            return false;
        }

        public void UpdateConnectivity()
        {
            dbutil.BulkUpdateConnectivity(_sectionList.Values.ToList());
        }

        public bool DownsectionPCCompatible(int parentPC, int childPC)
        {
            if (PhaseCodeUtil.IsPhaseCodePresent(parentPC, childPC))
                return true;
            else return false;
        }

        public bool ConnectionBuffer(double x1, double y1, double a, double b, double x2, double y2, Section dr, bool populatingUnenergized = false, bool mark128 = false)
        {
            if (dr.uid == "{9f9bb3e5-95ff-42c3-88af-a5e109004196}")
            {

            }
            if (dr.uid == "{705ec8f6-0176-4d7f-8820-4ad83bd92337}")
            {

            }
            int existingStatus = dr.status;
            bool ans1 = ((Math.Pow(x1 - a, 2) + Math.Pow(y1 - b, 2)) < (ntInfo.snappingTolerance * ntInfo.snappingTolerance));
            bool ans2 = ((Math.Pow(x2 - a, 2) + Math.Pow(y2 - b, 2)) < (ntInfo.snappingTolerance * ntInfo.snappingTolerance));
            //dr.status = StatusExtensions.Connect(existingStatus);
            if (!populatingUnenergized)
            {
                if (ans1 && existingStatus == (Constants.Disconnected))
                {
                    dr.status = Constants.WithFlow + Constants.Energized; //with flow
                }
                else if (ans2 && existingStatus == (Constants.Disconnected))
                {
                    dr.status = Constants.AgainstFlow + Constants.Energized; //against the flow
                }
                else
                {
                    if (ans1)
                    {
                        dr.status = Constants.WithFlow + Constants.Energized; //with flow
                    }
                    else if (ans2)
                    {
                        dr.status = Constants.AgainstFlow + Constants.Energized; //against the flow
                    }
                }
            }
            else
            {
                if (ans1 && existingStatus == (Constants.Disconnected))
                {
                    dr.status = Constants.WithFlow + Constants.Unenergized; //with flow
                }
                else if (ans2 && existingStatus == (Constants.Disconnected))
                {
                    dr.status = Constants.AgainstFlow + Constants.Unenergized; //against the flow
                }
                else
                {
                    if (ans1)
                    {
                        dr.status = Constants.WithFlow + Constants.Unenergized; //with flow
                    }
                    else if (ans2)
                    {
                        dr.status = Constants.AgainstFlow + Constants.Unenergized; //against the flow
                    }
                }
                if (mark128 == true) dr.phaseCode = 128;
            }
            return ans1 || ans2;
        }

        public bool ConnectionBufferDisconnected(double x1, double y1, double a, double b, double x2, double y2, Section dr)
        {
            int existingStatus = dr.status;
            bool ans1 = ((Math.Pow(x1 - a, 2) + Math.Pow(y1 - b, 2)) < (ntInfo.snappingTolerance * ntInfo.snappingTolerance));
            bool ans2 = ((Math.Pow(x2 - a, 2) + Math.Pow(y2 - b, 2)) < (ntInfo.snappingTolerance * ntInfo.snappingTolerance));
            //dr.status = StatusExtensions.Connect(existingStatus);
            if (ans1 && existingStatus == (Constants.Disconnected))
            {
                dr.status = Constants.WithFlow + Constants.Disconnected;
            }
            else if (ans2 && existingStatus == (Constants.Disconnected))
            {
                dr.status = Constants.AgainstFlow + Constants.Disconnected;
            }
            else
            {
                if (ans1)
                {
                    dr.status = Constants.WithFlow + Constants.Disconnected;
                }
                else if (ans2)
                {
                    dr.status = Constants.AgainstFlow + Constants.Disconnected;
                }
            }
            return ans1 || ans2;
        }

        public bool ConnectionBuffer(double x1, double y1, double a, double b, double x2, double y2)
        {
            bool ans1 = ((Math.Pow(x1 - a, 2) + Math.Pow(y1 - b, 2)) < (ntInfo.snappingTolerance * ntInfo.snappingTolerance));
            bool ans2 = ((Math.Pow(x2 - a, 2) + Math.Pow(y2 - b, 2)) < (ntInfo.snappingTolerance * ntInfo.snappingTolerance));
            return ans1 || ans2;
        }

        public void UpdateNodeStatus(Node nd, bool populatingUnenergized = false, bool mark128 = false)
        {
            if (nd != null)
            {
                int existingStatus = nd.status;
                nd.status = StatusExtensions.Connect(existingStatus);
                if (existingStatus != Constants.NetJunction)
                {
                    if (!populatingUnenergized)
                    {
                        if (existingStatus == (Constants.Disabled + Constants.Disconnected))
                        {
                            nd.status += Constants.Unenergized + Constants.Disabled;
                        }
                        else if (existingStatus == (Constants.Disconnected))
                        {
                            nd.status += Constants.Energized;
                        }
                    }
                    else
                    {
                        if (existingStatus == (Constants.Disabled + Constants.Disconnected))
                        {
                            nd.status += Constants.Unenergized + Constants.Disabled;
                        }
                        else if (existingStatus == (Constants.Disconnected))
                        {
                            nd.status += Constants.Unenergized;
                        }
                        if (mark128) nd.phaseCode = 128;
                    }
                }
            }
        }

        public void UpdateNetJnStatus(Node nd, int parentSecStatus)
        {
            if (nd != null)
            {
                if (StatusExtensions.IsUnenergized(parentSecStatus))
                {
                    nd.status = Constants.Unenergized + Constants.NetJunction;
                }
                else if (StatusExtensions.IsEnergized(parentSecStatus))
                {
                    nd.status = Constants.Energized + Constants.NetJunction;
                }
            }
        }

        public bool NodeBuffer(double x1, double y1, double a, double b)
        {
            bool ans = ((Math.Pow(x1 - a, 2) + Math.Pow(y1 - b, 2)) < (ntInfo.snappingTolerance * ntInfo.snappingTolerance));
            return ans;
        }

        public bool ConnExists(string guid)
        {
            bool exists = false;
            if (_sectionGUIDList.Contains(guid))
                exists = true;
            return exists;
        }

        public bool NodeExists(string guid)
        {
            bool exists = false;
            if (_nodeGUIDList.Contains(guid))
                exists = true;
            return exists;
        }

        public void BuildDisconnected(Queue<Node> q, Dictionary<Point, List<Node>> pointList, Dictionary<Point, List<Section>> lineList)
        {
            while (q.Count > 0)
            {
                bool isAdjNode = false;
                Node pnd = q.Dequeue();
                Node adjNode = null;
                string adjUID = "";
                if (pnd != null)
                {
                    double x1 = pnd.x;
                    double y1 = pnd.y;
                    int parentPhaseCode = pnd.phaseCode;
                    int xFloor = Convert.ToInt32(Math.Floor(x1));
                    int yFloor = Convert.ToInt32(Math.Floor(y1));
                    Point Pt1 = new Point(xFloor, yFloor);
                    var nearestSections = new List<Section>();
                    if (lineList.ContainsKey(Pt1)) nearestSections = lineList[Pt1];
                    var strtSec = from row in nearestSections
                                  where !ConnExists(row.uid) &&
                                  (ConnectionBufferDisconnected(row.x, row.y, x1, y1, row.tox, row.toy, row))
                                  select row;
                    foreach (var item in strtSec)
                    {
                        if (ClassIDRules.IsAdjJunction(pnd))
                        {
                            if (!ClassIDRules.AdjRuleSucceeds(pnd, item)) continue;
                        }
                        Section sect = null;
                        Node cnd = null;
                        string sectGUID = item.uid;
                        _sectionGUIDList.Add(sectGUID);
                        double toX = item.tox;
                        double toY = item.toy;
                        if (StatusExtensions.IsAgainstFlow(item.status))
                        {
                            toX = item.x;
                            toY = item.y;
                        }
                        int ToxFloor = Convert.ToInt32(Math.Floor(toX));
                        int ToyFloor = Convert.ToInt32(Math.Floor(toY));
                        Point currentPt = new Point(ToxFloor, ToyFloor);
                        var nearestPoints = new List<Node>();
                        if (pointList.ContainsKey(currentPt)) nearestPoints = pointList[currentPt];
                        var childNodes = from row in nearestPoints
                                         where (NodeBuffer(row.x, row.y, toX, toY))
                                         select row;
                        string Child = null;
                        int CNPhaseCode = 0;
                        int CNStatus = Constants.Disconnected;
                        string uid = sectGUID;
                        if (!_sectionList.ContainsKey(sectGUID))
                        {
                            sect = new Section();
                            _sectionList.Add(sect.uid, sect);
                            sect.uid = uid;
                        }
                        else
                        {
                            sect = _sectionList[sectGUID];
                        }
                        int status = item.status;
                        int phaseCode = item.phaseCode;
                        sect.phaseCode = phaseCode;
                        sect.status = status;
                        if (childNodes.Count() == 0)
                        {
                            double childX = toX;
                            double childY = toY;
                            Guid id = Guid.NewGuid();
                            Child = "{" + id.ToString() + "}";
                            CNPhaseCode = sect.phaseCode;
                            cnd = new Node(Child, CNStatus, CNPhaseCode);
                            UpdateNetJnStatus(cnd, status);
                            CNStatus = cnd.status;
                            cnd.phaseCode = CNPhaseCode;
                            cnd.classID = -1;//ntInfo.networkClassIds[ClassIDRules.NetJunction];
                            cnd.oid = -1;
                            cnd.x = childX;
                            cnd.y = childY;
                            AddPointsToPointList(cnd, pointList);
                            _nodeList.Add(Child, cnd);
                            q.Enqueue(cnd);
                            DataRow drNetJn = dtNetJnList.NewRow();
                            drNetJn["x"] = childX;
                            drNetJn["y"] = childY;
                            drNetJn["Tox"] = DBNull.Value;
                            drNetJn["Toy"] = DBNull.Value;
                            drNetJn["oid"] = "-1";
                            drNetJn["uid"] = Child;
                            drNetJn["adjuid"] = DBNull.Value;
                            drNetJn["phaseCode"] = CNPhaseCode;
                            drNetJn["status"] = CNStatus;
                            drNetJn["type"] = 0;
                            drNetJn["ClassID"] = -1;//ntInfo.networkClassIds[ClassIDRules.NetJunction];
                            dtNetJnList.Rows.Add(drNetJn);
                        }
                        else
                        {
                            foreach (var drChild in childNodes)//always one child will be considered
                            {
                                if (drChild.isAdj) { continue; }
                                if (ClassIDRules.IsAdjGroupNode(drChild))
                                {
                                    drChild.isAdj = true;
                                    double childX = toX;
                                    double childY = toY;
                                    Guid id = Guid.NewGuid();
                                    Child = "{" + id.ToString() + "}";
                                    CNPhaseCode = sect.phaseCode;
                                    cnd = new Node(Child, CNStatus, CNPhaseCode);
                                    UpdateNetJnStatus(cnd, status);
                                    CNStatus = cnd.status;
                                    cnd.phaseCode = CNPhaseCode;
                                    cnd.classID = -1;//ntInfo.networkClassIds[ClassIDRules.NetJunction];
                                    cnd.oid = -1;
                                    cnd.x = childX;
                                    cnd.y = childY;
                                    AddPointsToPointList(cnd, pointList);
                                    _nodeList.Add(Child, cnd);
                                    q.Enqueue(cnd);
                                    DataRow drNetJn = dtNetJnList.NewRow();
                                    drNetJn["x"] = childX;
                                    drNetJn["y"] = childY;
                                    drNetJn["Tox"] = DBNull.Value;
                                    drNetJn["Toy"] = DBNull.Value;
                                    drNetJn["oid"] = "-1";
                                    drNetJn["uid"] = Child;
                                    drNetJn["adjuid"] = drChild.uid;
                                    drNetJn["phaseCode"] = CNPhaseCode;
                                    drNetJn["status"] = CNStatus;
                                    drNetJn["type"] = 0;
                                    drNetJn["ClassID"] = -1;
                                    dtNetJnList.Rows.Add(drNetJn);
                                    cnd.adjacentNode = drChild;
                                    q.Enqueue(drChild);
                                }
                                else
                                {
                                    cnd = drChild;
                                    Child = drChild.uid;
                                    q.Enqueue(drChild);
                                }
                                break;
                            }
                        }
                        sect.parentNode = pnd;
                        sect.childNode = cnd;
                        if (pnd.childList.ContainsKey(sect.uid))
                        {
                            pnd.childList.Remove(sect.uid);
                            pnd.childList.Add(sect.uid, sect);
                        }
                        else pnd.childList.Add(sect.uid, sect);
                        if (cnd.parentList.ContainsKey(sect.uid))
                        {
                            cnd.parentList.Remove(sect.uid);
                            cnd.parentList.Add(sect.uid, sect);
                        }
                        else cnd.parentList.Add(sect.uid, sect);
                        if (ContainsAdjNode(cnd))
                        {
                            if (cnd.adjacentNode.parentList.ContainsKey(sect.uid))
                            {
                                cnd.adjacentNode.parentList.Remove(sect.uid);
                                cnd.adjacentNode.parentList.Add(sect.uid, sect);
                            }
                            else cnd.adjacentNode.parentList.Add(sect.uid, sect);
                        }
                    }
                }
            }
        }

        public void BuildDisconnected(Dictionary<Point, List<Node>> pointList, Dictionary<Point, List<Section>> lineList)
        {
            Queue<Node> q = new Queue<Node>();
            foreach (Section sec in _sectionList.Values)
            {
                if (StatusExtensions.IsDisconnected(sec.status))
                {
                    if (sec.uid == "{ab628494-bd05-4969-9f47-ebec7726840b}")
                    {

                    }
                    if (!ConnExists(sec.uid))
                    {
                        int xFloor = Convert.ToInt32(Math.Floor(sec.x));
                        int yFloor = Convert.ToInt32(Math.Floor(sec.y));
                        int toXFloor = Convert.ToInt32(Math.Floor(sec.tox));
                        int toYFloor = Convert.ToInt32(Math.Floor(sec.toy));
                        Point currentPt = new Point(xFloor, yFloor);
                        var nearestSections = new List<Section>();
                        if (lineList.ContainsKey(currentPt)) nearestSections = lineList[currentPt];
                        var connectedFromSections = from row in nearestSections
                                                    where
                                                    (ConnectionBuffer(row.x, row.y, sec.x, sec.y, row.tox, row.toy))
                                                    select row;
                        if (connectedFromSections.Count() == 1)
                        {
                            var nearestPoints = new List<Node>();
                            if (pointList.ContainsKey(currentPt)) nearestPoints = pointList[currentPt];
                            var connectedNodes = from row in nearestPoints
                                                 where (NodeBuffer(row.x, row.y, sec.x, sec.y))
                                                 select row;
                            Node rootNode = null;
                            if (connectedNodes.Count() == 0)
                            {
                                double rootX = sec.x;
                                double rootY = sec.y;
                                Guid id = Guid.NewGuid();
                                string guid = "{" + id.ToString() + "}";
                                int phaseCode = sec.phaseCode;
                                rootNode = new Node(guid, Constants.NetJunction, phaseCode);
                                UpdateNetJnStatus(rootNode, Constants.NetJunction);
                                int status = rootNode.status;
                                rootNode.phaseCode = phaseCode;
                                rootNode.classID = -1;
                                rootNode.oid = -1;
                                rootNode.x = rootX;
                                rootNode.y = rootY;
                                AddPointsToPointList(rootNode, pointList);
                                _nodeList.Add(guid, rootNode);
                                DataRow drNetJn = dtNetJnList.NewRow();
                                drNetJn["x"] = rootX;
                                drNetJn["y"] = rootY;
                                drNetJn["Tox"] = DBNull.Value;
                                drNetJn["Toy"] = DBNull.Value;
                                drNetJn["oid"] = "-1";
                                drNetJn["uid"] = guid;
                                drNetJn["adjuid"] = DBNull.Value;
                                drNetJn["phaseCode"] = phaseCode;
                                drNetJn["status"] = status;
                                drNetJn["type"] = 0;
                                drNetJn["ClassID"] = -1;
                                dtNetJnList.Rows.Add(drNetJn);
                            }
                            /*single child*/
                            else
                            {
                                rootNode = connectedNodes.FirstOrDefault();
                            }
                            q.Enqueue(rootNode);
                            BuildDisconnected(q, pointList, lineList);
                        }
                        else
                        {
                            currentPt = new Point(toXFloor, toYFloor);
                            nearestSections = new List<Section>();
                            if (lineList.ContainsKey(currentPt)) nearestSections = lineList[currentPt];
                            var connectedToSections = from row in nearestSections
                                                      where
                                                      (ConnectionBuffer(row.x, row.y, sec.tox, sec.toy, row.tox, row.toy))
                                                      select row;
                            if (connectedToSections.Count() == 1)
                            {
                                var nearestPoints = new List<Node>();
                                if (pointList.ContainsKey(currentPt)) nearestPoints = pointList[currentPt];
                                var connectedNodes = from row in nearestPoints
                                                     where (NodeBuffer(row.x, row.y, sec.tox, sec.toy))
                                                     select row;
                                Node rootNode = null;
                                if (connectedNodes.Count() == 0)
                                {
                                    double rootX = sec.tox;
                                    double rootY = sec.toy;
                                    Guid id = Guid.NewGuid();
                                    string guid = "{" + id.ToString() + "}";
                                    int phaseCode = sec.phaseCode;
                                    rootNode = new Node(guid, Constants.NetJunction, phaseCode);
                                    UpdateNetJnStatus(rootNode, Constants.NetJunction);
                                    int status = rootNode.status;
                                    rootNode.phaseCode = phaseCode;
                                    rootNode.classID = -1;
                                    rootNode.oid = -1;
                                    rootNode.x = rootX;
                                    rootNode.y = rootY;
                                    AddPointsToPointList(rootNode, pointList);
                                    _nodeList.Add(guid, rootNode);
                                    DataRow drNetJn = dtNetJnList.NewRow();
                                    drNetJn["x"] = rootX;
                                    drNetJn["y"] = rootY;
                                    drNetJn["Tox"] = DBNull.Value;
                                    drNetJn["Toy"] = DBNull.Value;
                                    drNetJn["oid"] = "-1";
                                    drNetJn["uid"] = guid;
                                    drNetJn["adjuid"] = DBNull.Value;
                                    drNetJn["phaseCode"] = phaseCode;
                                    drNetJn["status"] = status;
                                    drNetJn["type"] = 0;
                                    drNetJn["ClassID"] = -1;
                                    dtNetJnList.Rows.Add(drNetJn);
                                }
                                /*single child*/
                                else
                                {
                                    rootNode = connectedNodes.FirstOrDefault();
                                }
                                q.Enqueue(rootNode);
                                BuildDisconnected(q, pointList, lineList);
                            }
                        }
                    }
                }
            }
        }

        //public bool IfNodeInCompatibleWithParent(Node nd)
        //{
        //    int parentPC = nd.phaseCode;
        //    int parentSmPC = NetworkUtil.GetParentPCSum(nd);
        //    if (StatusExtensions.IsNetJunction(nd.status))
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        if (!StatusExtensions.IsYDTransformer(nd.status) && PhaseCodeUtil.IsPhaseCodePresent(parentPC, parentSmPC)) return true;
        //        if (PhaseCodeUtil.IsPhaseCodePresent(childPC, parentPC)) return true;
        //        else if (!PhaseCodeUtil.HaveCommonPhaseCodes(childPC, parentPC)) return false;
        //    }
        //    return false;
        //}

        public bool NotIncompatible(Node nd, int childPC)
        {
            int parentPC = nd.phaseCode;
            int parentSmPC = NetworkUtil.GetParentPCSum(nd);
            if (StatusExtensions.IsNetJunction(nd.status))
            {
                parentPC = parentSmPC;

            }
            if (PhaseCodeUtil.IsPhaseCodePresent(childPC, parentPC)) return true;
            else if (!PhaseCodeUtil.HaveCommonPhaseCodes(childPC, parentPC)) return false;
            return false;
        }

        public void BuildPhaseIncompatibleSection(Node strtNode, List<Node> disabledList, Dictionary<Point, List<Node>> pointList, Dictionary<Point, List<Section>> lineList, bool buildInCompatible = false)
        {
            Queue<Node> q = new Queue<Node>();
            if (strtNode != null)
            {
                q.Enqueue(strtNode);
                while (q.Count > 0)
                {
                    Node pnd = q.Dequeue();
                    if (pnd != null)
                    {
                        if (pnd.uid == "{9539f02b-8fa8-4a6e-8f12-ca630422009b}")
                        {

                        }
                        if (!StatusExtensions.IsDisabled(pnd.status))
                        {
                            double x1 = pnd.x;
                            double y1 = pnd.y;
                            int parentPhaseCode = pnd.phaseCode;
                            int xFloor = Convert.ToInt32(Math.Floor(x1));
                            int yFloor = Convert.ToInt32(Math.Floor(y1));
                            Point Pt1 = new Point(xFloor, yFloor);
                            var nearestSections = new List<Section>();
                            if (lineList.ContainsKey(Pt1)) nearestSections = lineList[Pt1];
                            var strtSec = from row in nearestSections
                                          where !ConnExists(row.uid) &&
                                          (ConnectionBuffer(row.x, row.y, x1, y1, row.tox, row.toy, row, true, false))
                                          select row;
                            foreach (var item in strtSec)
                            {
                                if (item.uid == "{9a7bdd5e-7203-4274-aa50-521306474f07}")
                                {

                                }
                                if (item.uid == "{b168c622-48f7-487d-afbb-193bbe730b7c}")
                                {

                                }
                                if (item.uid == "{050dd67b-3b9b-4f4d-9a84-d5b24eebdb52}")
                                {

                                }
                                if (ClassIDRules.IsAdjJunction(pnd))
                                {
                                    if (!ClassIDRules.AdjRuleSucceeds(pnd, item))
                                    {
                                        continue;
                                    }
                                }
                                Section sect = null;
                                Node cnd = null;
                                string sectGUID = item.uid;
                                
                                
                                if (!NetworkUtil.VerifyNodePCWithParents(pnd, -1, false))
                                {
                                    pnd.phaseCode = NetworkUtil.GetParentPCSum(pnd);
                                }
                                if (!NotIncompatible(pnd, item.phaseCode) && !buildInCompatible)
                                {
                                    phaseErrorQueue.Add(pnd);
                                    continue;
                                }
                                //NetworkUtil.DownsectionPCAdjustment(pnd, item);//not here later
                                _sectionGUIDList.Add(sectGUID);
                                double toX = item.tox;
                                double toY = item.toy;
                                if (StatusExtensions.IsAgainstFlow(item.status))
                                {
                                    toX = item.x;
                                    toY = item.y;
                                }
                                int ToxFloor = Convert.ToInt32(Math.Floor(toX));
                                int ToyFloor = Convert.ToInt32(Math.Floor(toY));
                                Point currentPt = new Point(ToxFloor, ToyFloor);
                                var nearestPoints = new List<Node>();
                                if (pointList.ContainsKey(currentPt)) nearestPoints = pointList[currentPt];
                                var childNodes = from row in nearestPoints
                                                 where (NodeBuffer(row.x, row.y, toX, toY))
                                                 select row;
                                string Child = null;
                                int CNPhaseCode = 0;
                                int CNStatus = Constants.Disconnected;
                                string uid = sectGUID;
                                if (!_sectionList.ContainsKey(sectGUID))
                                {
                                    sect = new Section();
                                    _sectionList.Add(sect.uid, sect);
                                    sect.uid = uid;
                                }
                                else
                                {
                                    sect = _sectionList[sectGUID];
                                }
                                int status = item.status;
                                int phaseCode = item.phaseCode;
                                sect.phaseCode = phaseCode;
                                sect.status = status;
                                if (childNodes.Count() == 0)
                                {
                                    double childX = toX;
                                    double childY = toY;
                                    Guid id = Guid.NewGuid();
                                    Child = "{" + id.ToString() + "}";
                                    CNPhaseCode = sect.phaseCode;
                                    cnd = new Node(Child, CNStatus, CNPhaseCode);
                                    UpdateNetJnStatus(cnd, status);
                                    CNStatus = cnd.status;
                                    cnd.phaseCode = CNPhaseCode;
                                    cnd.classID = -1;//ntInfo.networkClassIds[ClassIDRules.NetJunction];
                                    cnd.oid = -1;
                                    cnd.x = childX;
                                    cnd.y = childY;
                                    AddPointsToPointList(cnd, pointList);
                                    _nodeList.Add(Child, cnd);
                                    q.Enqueue(cnd);
                                    DataRow drNetJn = dtNetJnList.NewRow();
                                    drNetJn["x"] = childX;
                                    drNetJn["y"] = childY;
                                    drNetJn["Tox"] = DBNull.Value;
                                    drNetJn["Toy"] = DBNull.Value;
                                    drNetJn["oid"] = "-1";
                                    drNetJn["uid"] = Child;
                                    drNetJn["adjuid"] = DBNull.Value;
                                    drNetJn["phaseCode"] = CNPhaseCode;
                                    drNetJn["constructedphaseCode"] = CNPhaseCode;
                                    drNetJn["status"] = CNStatus;
                                    drNetJn["type"] = 0;
                                    drNetJn["ClassID"] = -1;//ntInfo.networkClassIds[ClassIDRules.NetJunction];
                                    dtNetJnList.Rows.Add(drNetJn);
                                }
                                else
                                {
                                    foreach (var drChild in childNodes)//always one child will be considered
                                    {
                                        if (drChild.uid == "{1669a2ce-2e60-460d-95eb-8e1e9fc07c15}")
                                        {

                                        }
                                        if (drChild.isAdj) 
                                        {
                                            continue; }
                                        if (ClassIDRules.IsAdjGroupNode(drChild))
                                        {
                                            drChild.isAdj = true;
                                            double childX = toX;
                                            double childY = toY;
                                            Guid id = Guid.NewGuid();
                                            Child = "{" + id.ToString() + "}";
                                            CNPhaseCode = sect.phaseCode;
                                            cnd = new Node(Child, CNStatus, CNPhaseCode);
                                            UpdateNetJnStatus(cnd, status);
                                            CNStatus = cnd.status;
                                            cnd.phaseCode = CNPhaseCode;
                                            cnd.classID = -1;
                                            cnd.oid = -1;
                                            cnd.x = childX;
                                            cnd.y = childY;
                                            AddPointsToPointList(cnd, pointList);
                                            _nodeList.Add(Child, cnd);
                                            q.Enqueue(cnd);
                                            DataRow drNetJn = dtNetJnList.NewRow();
                                            drNetJn["x"] = childX;
                                            drNetJn["y"] = childY;
                                            drNetJn["Tox"] = DBNull.Value;
                                            drNetJn["Toy"] = DBNull.Value;
                                            drNetJn["oid"] = "-1";
                                            drNetJn["uid"] = Child;
                                            drNetJn["adjuid"] = drChild.uid;
                                            drNetJn["phaseCode"] = CNPhaseCode;
                                            drNetJn["constructedphaseCode"] = CNPhaseCode;
                                            drNetJn["status"] = CNStatus;
                                            drNetJn["type"] = 0;
                                            drNetJn["ClassID"] = -1;
                                            dtNetJnList.Rows.Add(drNetJn);
                                            cnd.adjacentNode = drChild;
                                            q.Enqueue(drChild);
                                        }
                                        else
                                        {
                                            UpdateNodeStatus(drChild, true);
                                            cnd = drChild;
                                            Child = drChild.uid;
                                            q.Enqueue(drChild);
                                        }
                                        break;
                                    }
                                }
                                UpdateNodeStatus(pnd, true, false);
                                sect.parentNode = pnd;
                                sect.childNode = cnd;
                                if (pnd.childList.ContainsKey(sect.uid))
                                {
                                    pnd.childList.Remove(sect.uid);
                                    pnd.childList.Add(sect.uid, sect);
                                }
                                else pnd.childList.Add(sect.uid, sect);
                                if (cnd.parentList.ContainsKey(sect.uid))
                                {
                                    cnd.parentList.Remove(sect.uid);
                                    cnd.parentList.Add(sect.uid, sect);
                                }
                                else cnd.parentList.Add(sect.uid, sect);
                                if (ContainsAdjNode(cnd))
                                {
                                    if (cnd.adjacentNode.parentList.ContainsKey(sect.uid))
                                    {
                                        cnd.adjacentNode.parentList.Remove(sect.uid);
                                        cnd.adjacentNode.parentList.Add(sect.uid, sect);
                                    }
                                    else cnd.adjacentNode.parentList.Add(sect.uid, sect);
                                }
                            }
                        }
                        else
                        {
                            disabledList.Add(pnd);
                        }
                    }
                }
            }
        }

        public void BuildConnection(Node strtNode, List<Node> disabledList, Dictionary<Point, List<Node>> pointList, Dictionary<Point, List<Section>> lineList, bool BuildUnenergized)
        {
            bool firstNode = true;
            Queue<Node> q = new Queue<Node>();
            if (strtNode != null)
            {
                q.Enqueue(strtNode);
                while (q.Count > 0)
                {
                    Node pnd = q.Dequeue();
                    if (pnd != null)
                    {
                        if (pnd.uid == "{2848a674-18a9-4238-bd97-943a3c6f0f15}")
                        {

                        }
                        if (!StatusExtensions.IsDisabled(pnd.status) || (firstNode && BuildUnenergized))
                        {
                            firstNode = false;
                            double x1 = pnd.x;
                            double y1 = pnd.y;
                            int parentPhaseCode = pnd.phaseCode;
                            int xFloor = Convert.ToInt32(Math.Floor(x1));
                            int yFloor = Convert.ToInt32(Math.Floor(y1));
                            Point Pt1 = new Point(xFloor, yFloor);
                            var nearestSections = new List<Section>();
                            if (lineList.ContainsKey(Pt1)) nearestSections = lineList[Pt1];
                            var strtSec = from row in nearestSections
                                          where !ConnExists(row.uid) &&
                                          (ConnectionBuffer(row.x, row.y, x1, y1, row.tox, row.toy, row, BuildUnenergized))
                                          select row;
                            foreach (var item in strtSec)
                            {
                                if (ClassIDRules.IsAdjJunction(pnd))
                                {
                                    if (!ClassIDRules.AdjRuleSucceeds(pnd, item)) continue;
                                }
                                Section sect = null;
                                Node cnd = null;
                                string sectGUID = item.uid;
                                if (sectGUID == "{705ec8f6-0176-4d7f-8820-4ad83bd92337}")
                                {

                                }
                                if (sectGUID == "{9f9bb3e5-95ff-42c3-88af-a5e109004196}")
                                {

                                }
                                if (sectGUID == "{050dd67b-3b9b-4f4d-9a84-d5b24eebdb52}")
                                {

                                }
                                if (NetworkUtil.DownsectionPCCompatible(pnd, item.phaseCode, -1, false))
                                {
                                    _sectionGUIDList.Add(sectGUID);
                                    double toX = item.tox;
                                    double toY = item.toy;
                                    if (StatusExtensions.IsAgainstFlow(item.status))
                                    {
                                        toX = item.x;
                                        toY = item.y;
                                    }
                                    int ToxFloor = Convert.ToInt32(Math.Floor(toX));
                                    int ToyFloor = Convert.ToInt32(Math.Floor(toY));
                                    Point currentPt = new Point(ToxFloor, ToyFloor);
                                    var nearestPoints = new List<Node>();
                                    if (pointList.ContainsKey(currentPt)) nearestPoints = pointList[currentPt];
                                    var childNodes = from row in nearestPoints
                                                     where (NodeBuffer(row.x, row.y, toX, toY))
                                                     select row;
                                    string Child = null;
                                    int CNPhaseCode = 0;
                                    int CNStatus = Constants.Disconnected;
                                    string uid = sectGUID;
                                    if (!_sectionList.ContainsKey(sectGUID))
                                    {
                                        sect = new Section();
                                        _sectionList.Add(sect.uid, sect);
                                        sect.uid = uid;
                                    }
                                    else
                                    {
                                        sect = _sectionList[sectGUID];
                                    }
                                    int status = item.status;
                                    int phaseCode = item.phaseCode;
                                    sect.phaseCode = phaseCode;
                                    sect.status = status;
                                    if (childNodes.Count() == 0)
                                    {
                                        double childX = toX;
                                        double childY = toY;
                                        Guid id = Guid.NewGuid();
                                        Child = "{" + id.ToString() + "}";
                                        CNPhaseCode = sect.phaseCode;
                                        cnd = new Node(Child, CNStatus, CNPhaseCode);
                                        UpdateNetJnStatus(cnd, status);
                                        CNStatus = cnd.status;
                                        cnd.phaseCode = CNPhaseCode;
                                        cnd.classID = -1;//ntInfo.networkClassIds[ClassIDRules.NetJunction];
                                        cnd.oid = -1;
                                        cnd.x = childX;
                                        cnd.y = childY;
                                        AddPointsToPointList(cnd, pointList);
                                        _nodeList.Add(Child, cnd);
                                        q.Enqueue(cnd);
                                        DataRow drNetJn = dtNetJnList.NewRow();
                                        drNetJn["x"] = childX;
                                        drNetJn["y"] = childY;
                                        drNetJn["Tox"] = DBNull.Value;
                                        drNetJn["Toy"] = DBNull.Value;
                                        drNetJn["oid"] = "-1";
                                        drNetJn["uid"] = Child;
                                        drNetJn["adjuid"] = DBNull.Value;
                                        drNetJn["phaseCode"] = CNPhaseCode;
                                        drNetJn["constructedphaseCode"] = CNPhaseCode;
                                        drNetJn["status"] = CNStatus;
                                        drNetJn["type"] = 0;
                                        drNetJn["ClassID"] = -1;//ntInfo.networkClassIds[ClassIDRules.NetJunction];
                                        dtNetJnList.Rows.Add(drNetJn);
                                    }
                                    else
                                    {
                                        foreach (var drChild in childNodes)//always one child will be considered
                                        {
                                            if (drChild.uid == "{1669a2ce-2e60-460d-95eb-8e1e9fc07c15}")
                                            {

                                            }
                                            if (drChild.isAdj) 
                                            { 
                                                continue; }
                                            if (ClassIDRules.IsAdjGroupNode(drChild))
                                            {
                                                drChild.isAdj = true;
                                                double childX = toX;
                                                double childY = toY;
                                                Guid id = Guid.NewGuid();
                                                Child = "{" + id.ToString() + "}";
                                                CNPhaseCode = sect.phaseCode;
                                                cnd = new Node(Child, CNStatus, CNPhaseCode);
                                                UpdateNetJnStatus(cnd, status);
                                                CNStatus = cnd.status;
                                                cnd.phaseCode = CNPhaseCode;
                                                cnd.classID = -1;
                                                cnd.oid = -1;
                                                cnd.x = childX;
                                                cnd.y = childY;
                                                AddPointsToPointList(cnd, pointList);
                                                _nodeList.Add(Child, cnd);
                                                q.Enqueue(cnd);
                                                DataRow drNetJn = dtNetJnList.NewRow();
                                                drNetJn["x"] = childX;
                                                drNetJn["y"] = childY;
                                                drNetJn["Tox"] = DBNull.Value;
                                                drNetJn["Toy"] = DBNull.Value;
                                                drNetJn["oid"] = "-1";
                                                drNetJn["uid"] = Child;
                                                drNetJn["adjuid"] = drChild.uid;
                                                drNetJn["phaseCode"] = CNPhaseCode;
                                                drNetJn["constructedphaseCode"] = CNPhaseCode;
                                                drNetJn["status"] = CNStatus;
                                                drNetJn["type"] = 0;
                                                drNetJn["ClassID"] = -1;
                                                dtNetJnList.Rows.Add(drNetJn);
                                                cnd.adjacentNode = drChild;
                                                q.Enqueue(drChild);
                                            }
                                            else
                                            {
                                                UpdateNodeStatus(drChild, BuildUnenergized);
                                                cnd = drChild;
                                                Child = drChild.uid;
                                                q.Enqueue(drChild);
                                            }
                                            break;
                                        }
                                    }
                                    UpdateNodeStatus(pnd, BuildUnenergized);
                                    sect.parentNode = pnd;
                                    sect.childNode = cnd;
                                    if (pnd.childList.ContainsKey(sect.uid))
                                    {
                                        pnd.childList.Remove(sect.uid);
                                        pnd.childList.Add(sect.uid, sect);
                                    }
                                    else pnd.childList.Add(sect.uid, sect);
                                    if (cnd.parentList.ContainsKey(sect.uid))
                                    {
                                        cnd.parentList.Remove(sect.uid);
                                        cnd.parentList.Add(sect.uid, sect);
                                    }
                                    else cnd.parentList.Add(sect.uid, sect);
                                    if (ContainsAdjNode(cnd))
                                    {
                                        if (cnd.adjacentNode.parentList.ContainsKey(sect.uid))
                                        {
                                            cnd.adjacentNode.parentList.Remove(sect.uid);
                                            cnd.adjacentNode.parentList.Add(sect.uid, sect);
                                        }
                                        else cnd.adjacentNode.parentList.Add(sect.uid, sect);
                                    }
                                    if (NetworkUtil.DownsectionPCCompatibleWithSection(sect, cnd, -1, false))
                                    {
                                        phaseErrorList.Add(pnd);
                                        continue;
                                    }
                                }
                                else
                                {
                                    phaseErrorList.Add(pnd);
                                }
                            }
                        }
                        else
                        {
                            disabledList.Add(pnd);
                        }
                    }

                }
            }
        }

        public void BuildUnenergized(Node strtNode, List<Node> disabledList, Dictionary<Point, List<Node>> pointList, Dictionary<Point, List<Section>> lineList)
        {
            Queue<Node> q = new Queue<Node>();
            if (strtNode != null)
            {
                q.Enqueue(strtNode);
                while (q.Count > 0)
                {
                    Node pnd = q.Dequeue();
                    if (pnd != null)
                    {
                        if (pnd.uid == "{1669a2ce-2e60-460d-95eb-8e1e9fc07c15}")
                        {

                        }
                        if (pnd.uid == "{cd3ddfc0-8160-434c-af4a-2aa10f97e992}")
                        {

                        }
                        if (!StatusExtensions.IsDisabled(pnd.status))
                        {
                            double x1 = pnd.x;
                            double y1 = pnd.y;
                            int parentPhaseCode = pnd.phaseCode;
                            int xFloor = Convert.ToInt32(Math.Floor(x1));
                            int yFloor = Convert.ToInt32(Math.Floor(y1));
                            Point Pt1 = new Point(xFloor, yFloor);
                            var nearestSections = new List<Section>();
                            if (lineList.ContainsKey(Pt1)) nearestSections = lineList[Pt1];
                            var strtSec = from row in nearestSections
                                          where !ConnExists(row.uid) &&
                                          (ConnectionBuffer(row.x, row.y, x1, y1, row.tox, row.toy, row, true, true))
                                          select row;
                            foreach (var item in strtSec)
                            {
                                if (ClassIDRules.IsAdjJunction(pnd))
                                {
                                    if (!ClassIDRules.AdjRuleSucceeds(pnd, item)) continue;
                                }
                                Section sect = null;
                                Node cnd = null;
                                string sectGUID = item.uid;
                                if (sectGUID == "{9a7bdd5e-7203-4274-aa50-521306474f07}")
                                {

                                }
                                _sectionGUIDList.Add(sectGUID);
                                _correctedPCList.Add(sectGUID);
                                double toX = item.tox;
                                double toY = item.toy;
                                if (StatusExtensions.IsAgainstFlow(item.status))
                                {
                                    toX = item.x;
                                    toY = item.y;
                                }
                                int ToxFloor = Convert.ToInt32(Math.Floor(toX));
                                int ToyFloor = Convert.ToInt32(Math.Floor(toY));
                                Point currentPt = new Point(ToxFloor, ToyFloor);
                                var nearestPoints = new List<Node>();
                                if (pointList.ContainsKey(currentPt)) nearestPoints = pointList[currentPt];
                                var childNodes = from row in nearestPoints
                                                 where (NodeBuffer(row.x, row.y, toX, toY))
                                                 select row;
                                string Child = null;
                                int CNPhaseCode = 0;
                                int CNStatus = Constants.Disconnected;
                                string uid = sectGUID;
                                if (!_sectionList.ContainsKey(sectGUID))
                                {
                                    sect = new Section();
                                    _sectionList.Add(sect.uid, sect);
                                    sect.uid = uid;
                                }
                                else
                                {
                                    sect = _sectionList[sectGUID];
                                }
                                int status = item.status;
                                int phaseCode = item.phaseCode;
                                sect.phaseCode = phaseCode;
                                sect.status = status;
                                if (childNodes.Count() == 0)
                                {
                                    double childX = toX;
                                    double childY = toY;
                                    Guid id = Guid.NewGuid();
                                    Child = "{" + id.ToString() + "}";
                                    CNPhaseCode = sect.phaseCode;
                                    cnd = new Node(Child, CNStatus, CNPhaseCode);
                                    UpdateNetJnStatus(cnd, status);
                                    CNStatus = cnd.status;
                                    cnd.phaseCode = CNPhaseCode;
                                    cnd.classID = -1;//ntInfo.networkClassIds[ClassIDRules.NetJunction];
                                    cnd.oid = -1;
                                    cnd.x = childX;
                                    cnd.y = childY;
                                    AddPointsToPointList(cnd, pointList);
                                    _nodeList.Add(Child, cnd);
                                    q.Enqueue(cnd);
                                    DataRow drNetJn = dtNetJnList.NewRow();
                                    drNetJn["x"] = childX;
                                    drNetJn["y"] = childY;
                                    drNetJn["Tox"] = DBNull.Value;
                                    drNetJn["Toy"] = DBNull.Value;
                                    drNetJn["oid"] = "-1";
                                    drNetJn["uid"] = Child;
                                    drNetJn["adjuid"] = DBNull.Value;
                                    drNetJn["phaseCode"] = CNPhaseCode;
                                    drNetJn["constructedphaseCode"] = CNPhaseCode;
                                    drNetJn["status"] = CNStatus;
                                    drNetJn["type"] = 0;
                                    drNetJn["ClassID"] = -1;//ntInfo.networkClassIds[ClassIDRules.NetJunction];
                                    dtNetJnList.Rows.Add(drNetJn);
                                }
                                else
                                {
                                    foreach (var drChild in childNodes)//always one child will be considered
                                    {
                                        if (drChild.isAdj) { continue; }
                                        if (ClassIDRules.IsAdjGroupNode(drChild))
                                        {
                                            drChild.isAdj = true;
                                            double childX = toX;
                                            double childY = toY;
                                            Guid id = Guid.NewGuid();
                                            Child = "{" + id.ToString() + "}";
                                            CNPhaseCode = sect.phaseCode;
                                            cnd = new Node(Child, CNStatus, CNPhaseCode);
                                            UpdateNetJnStatus(cnd, status);
                                            CNStatus = cnd.status;
                                            cnd.phaseCode = CNPhaseCode;
                                            cnd.classID = -1;
                                            cnd.oid = -1;
                                            cnd.x = childX;
                                            cnd.y = childY;
                                            AddPointsToPointList(cnd, pointList);
                                            _nodeList.Add(Child, cnd);
                                            q.Enqueue(cnd);
                                            DataRow drNetJn = dtNetJnList.NewRow();
                                            drNetJn["x"] = childX;
                                            drNetJn["y"] = childY;
                                            drNetJn["Tox"] = DBNull.Value;
                                            drNetJn["Toy"] = DBNull.Value;
                                            drNetJn["oid"] = "-1";
                                            drNetJn["uid"] = Child;
                                            drNetJn["adjuid"] = drChild.uid;
                                            drNetJn["phaseCode"] = CNPhaseCode;
                                            drNetJn["constructedphaseCode"] = CNPhaseCode;
                                            drNetJn["status"] = CNStatus;
                                            drNetJn["type"] = 0;
                                            drNetJn["ClassID"] = -1;
                                            dtNetJnList.Rows.Add(drNetJn);
                                            cnd.adjacentNode = drChild;
                                            q.Enqueue(drChild);
                                        }
                                        else
                                        {
                                            UpdateNodeStatus(drChild, true, true);
                                            cnd = drChild;
                                            Child = drChild.uid;
                                            q.Enqueue(drChild);
                                        }
                                        break;
                                    }
                                }
                                UpdateNodeStatus(pnd, true, false);
                                sect.parentNode = pnd;
                                sect.childNode = cnd;
                                if (pnd.childList.ContainsKey(sect.uid))
                                {
                                    pnd.childList.Remove(sect.uid);
                                    pnd.childList.Add(sect.uid, sect);
                                }
                                else pnd.childList.Add(sect.uid, sect);
                                if (cnd.parentList.ContainsKey(sect.uid))
                                {
                                    cnd.parentList.Remove(sect.uid);
                                    cnd.parentList.Add(sect.uid, sect);
                                }
                                else cnd.parentList.Add(sect.uid, sect);
                                if (ContainsAdjNode(cnd))
                                {
                                    if (cnd.adjacentNode.parentList.ContainsKey(sect.uid))
                                    {
                                        cnd.adjacentNode.parentList.Remove(sect.uid);
                                        cnd.adjacentNode.parentList.Add(sect.uid, sect);
                                    }
                                    else cnd.adjacentNode.parentList.Add(sect.uid, sect);
                                }
                            }
                        }
                        else
                        {
                            disabledList.Add(pnd);
                        }
                    }

                }
            }
        }

        public void LogStuff(string message)
        {
            try
            {
                string path = @"C:\Temp\NetworkLog.txt";
                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                    using (TextWriter tw = new StreamWriter(path, true))
                    {
                        tw.WriteLine(DateTime.Now.ToString() + message);
                        tw.Close();
                    }
                }
                else if (File.Exists(path))
                {
                    using (TextWriter tw = new StreamWriter(path, true))
                    {
                        tw.WriteLine(DateTime.Now.ToString() + message);
                        tw.Close();
                    }
                }
            }
            catch (Exception exc)
            {
            }
        }

        public long GetObjectSize(object o)
        {
            long size = 0;
            using (Stream s = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, o);
                size = s.Length;
            }
            return size;
        }

        public bool AllParentsVisited(Node nd)
        {
            bool visited = true;
            foreach (Section sec in nd.parentList.Values)
            {
                if (!_correctedPCList.Contains(sec.uid))
                    visited = false;
            }
            return visited;
        }

        public void BuildCorrectedPhaseCode(Node strtNode)
        {
            Queue<Node> q = new Queue<Node>();
            q.Enqueue(strtNode);
            if (strtNode != null)
            {                
                while (q.Count > 0)
                {
                    Node current = q.Dequeue();
                    if (current.uid == "{1669a2ce-2e60-460d-95eb-8e1e9fc07c15}")
                    {

                    }
                    if (ContainsAdjNode(current))
                    {
                        q.Enqueue(current.adjacentNode);
                    }
                    if (AllParentsVisited(current))
                    {
                        int parentPC = NetworkUtil.GetParentPCSum(current);
                        if (!StatusExtensions.IsNetJunction(current.status) && PhaseCodeUtil.IsPhaseCodePresent(current.phaseCode, parentPC))
                            current.phaseCode = parentPC;
                        if (StatusExtensions.IsNetJunction(current.status))
                        {
                            parentPC = NetworkUtil.GetParentPCSum(current);
                        }
                        if (current == null || StatusExtensions.IsDisabled(current.status))
                            continue;
                        int parentPhase = current.phaseCode;
                        foreach (var item in current.childList.Values)
                        {
                            if (!_correctedPCList.Contains(item.uid))
                            {
                                NetworkUtil.DownsectionPCAdjustment(current, item);
                                q.Enqueue(item.childNode);
                                _correctedPCList.Add(item.uid);
                            }
                        }
                    }
                    else
                    {
                        phaseCorrectionList.Add(current);
                    }
                }
            }
        }

        public List<List<INetworkObject>> GroupUnenergizedIncompatibleSections()
        {
            HashSet<Section> secList = new HashSet<Section>();
            Dictionary<Node, HashSet<Section>> nodeAsParent = new Dictionary<Node, HashSet<Section>>();
            Dictionary<Node, HashSet<Section>> nodeAsChild = new Dictionary<Node, HashSet<Section>>();
            List<List<INetworkObject>> lists = new List<List<INetworkObject>>();
            foreach (Section sec in _sectionList.Values)
            {
                if (StatusExtensions.IsUnenergized(sec.status) && sec.phaseCode != 128)
                {
                    secList.Add(sec);
                    if (nodeAsParent.ContainsKey(sec.parentNode)) nodeAsParent[sec.parentNode].Add(sec);
                    else
                    {
                        HashSet<Section> newList = new HashSet<Section>();
                        newList.Add(sec);
                        nodeAsParent.Add(sec.parentNode, newList);
                    }
                    if (nodeAsChild.ContainsKey(sec.childNode)) nodeAsChild[sec.childNode].Add(sec);
                    else
                    {
                        HashSet<Section> newList = new HashSet<Section>();
                        newList.Add(sec);
                        nodeAsChild.Add(sec.childNode, newList);
                    }
                }
            }
            HashSet<Section> visitedSecList = new HashSet<Section>();
            foreach (Section sec in secList)
            {
                if (!visitedSecList.Contains(sec))
                {
                    List<INetworkObject> list = new List<INetworkObject>();
                    list.Add(sec);
                    visitedSecList.Add(sec);
                    Queue<Node> q = new Queue<Node>();
                    Node current = sec.childNode;
                    q.Enqueue(current);
                    while (q.Count > 0)
                    {
                        current = q.Dequeue();
                        if (nodeAsParent.ContainsKey(current))
                        {                            
                            list.Add(current);
                            foreach (var item in nodeAsParent[current])
                            {
                                if (!visitedSecList.Contains(item))
                                {
                                    visitedSecList.Add(item);
                                    Node child = item.childNode;
                                    q.Enqueue(child);
                                    list.Add(item);
                                }
                            }
                        }
                    }
                    current = sec.parentNode;
                    q.Enqueue(current);
                    while (q.Count > 0)
                    {
                        current = q.Dequeue();
                        if (nodeAsChild.ContainsKey(current))
                        {                            
                            list.Add(current);
                            foreach (var item in nodeAsChild[current])
                            {
                                if (!visitedSecList.Contains(item))
                                {
                                    visitedSecList.Add(item);
                                    Node parent = item.parentNode;
                                    q.Enqueue(parent);
                                    list.Add(item);
                                }
                            }
                        }
                    }
                    lists.Add(list);
                }
            }
            return lists;
        }

        public void Create()
        {
            StaticStuff.InitializeNTInfo();
            DateTime dtstart = DateTime.Now;
            LogStuff("before populate elements");
            dbutil.PopulateElements();
            dbutil.ApplySource();
            dbutil.ApplyTransformer();
            LogStuff("after populate elements");
            StaticStuff.FillElementsFromDB();
            LogStuff("after loading elements");
            string tableName = "DISTRIBUTIONSOURCE";
            string guidName = "FuturaGUID";
            DataTable dtsourceList = dbutil.GetSubstations(tableName, guidName);
            Dictionary<Point, List<Node>> pointList = new Dictionary<Point, List<Node>>();
            Dictionary<Point, List<Section>> lineList = new Dictionary<Point, List<Section>>();
            dtNetJnList.Clear();
            dtNetJnList.Columns.Add("id");
            dtNetJnList.Columns.Add("uid");
            dtNetJnList.Columns.Add("adjuid");
            dtNetJnList.Columns.Add("type");
            dtNetJnList.Columns.Add("oid");
            dtNetJnList.Columns.Add("x");
            dtNetJnList.Columns.Add("y");
            dtNetJnList.Columns.Add("tox");
            dtNetJnList.Columns.Add("toy");
            dtNetJnList.Columns.Add("status");
            dtNetJnList.Columns.Add("PhaseCode");
            dtNetJnList.Columns.Add("ConstructedPhaseCode");
            dtNetJnList.Columns.Add("SessionID");
            dtNetJnList.Columns.Add("deleted");
            dtNetJnList.Columns.Add("ClassID");
            long sizebefore = GC.GetTotalMemory(false);
            LogStuff("before loading Geometric Info");
            foreach (Section dr in _sectionList.Values)
            {
                AddPointsToLineList(dr, lineList);
            }
            foreach (Node dr in _nodeList.Values)
            {
                AddPointsToPointList(dr, pointList);
            }
            // long size = GetObjectSize(lineList) + GetObjectSize(pointList);
            LogStuff("after loading Geometric Info with size"); //" + size.ToString());
            long sizeafter = GC.GetTotalMemory(false);
            foreach (DataRow dr in dtsourceList.Rows)
            {
                string strtGuid = dr["futuraguid"] != null ? dr["futuraguid"].ToString() : "";
                Node strtNode = null;
                if (_nodeList.ContainsKey(strtGuid))
                {
                    strtNode = _nodeList[strtGuid];
                }
                BuildConnection(strtNode, disabledList, pointList, lineList, false);
            }
            _correctedPCList.UnionWith(_sectionGUIDList);
            phaseErrorQueue = new List<Node>(phaseErrorList);
            int initialCount = phaseErrorList.Count - 1;
            for (int i = 0; i < phaseErrorQueue.Count; i++)
            {
                if (i <= initialCount)
                    BuildPhaseIncompatibleSection(phaseErrorQueue[i], disabledList, pointList, lineList);
                else
                    BuildPhaseIncompatibleSection(phaseErrorQueue[i], disabledList, pointList, lineList, true);
            }

            for (int i = 0; i < disabledList.Count; i++)
            {
                BuildUnenergized(disabledList[i], disabledList, pointList, lineList);
            }
            BuildDisconnected(pointList, lineList);

            phaseCorrectionList = new List<Node>(phaseErrorList);
            for (int i = 0; i < phaseCorrectionList.Count; i++)
            {
                BuildCorrectedPhaseCode(phaseCorrectionList[i]);
            }

            List<List<INetworkObject>> incompatibleSectionGroups =  GroupUnenergizedIncompatibleSections();

            //dtNetJnList.AcceptChanges();
            LogStuff("after loading in memory network");
            NetworkUtil.PlaceOpenPoints(dtNetJnList);
            dbutil.BulkInsertElementNodes(dtNetJnList);
            LogStuff("after inserting net junctions");
            NetworkUtil.DetectandDisableLoops();
            LogStuff("after detecting loops and before updating connectivity");
            UpdateConnectivity();
            LogStuff("after updating connectivity");
            dbutil.UpdateStatusAndPhase();
            LogStuff("after updating phase and status");
        }


    }
}
