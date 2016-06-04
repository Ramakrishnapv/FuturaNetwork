using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FuturaNetwork
{
    public class CheckFlow
    {
        public static Dictionary<string, Node> _nodeList = StaticStuff._nodeList;
        public static Dictionary<string, Section> _sectionList = StaticStuff._sectionList;
        public static Dictionary<Point, List<Node>> pointList = new Dictionary<Point, List<Node>>();
        public static Dictionary<Point, List<Section>> lineList = new Dictionary<Point, List<Section>>();
        public static DBUtil dbutil = StaticStuff.dbutil;
        public NTInfo ntInfo = StaticStuff.ntInfo;
        public Dictionary<int, int> primaryStatuses = new Dictionary<int, int>();
        public Dictionary<int, int> secondaryStatuses = new Dictionary<int, int>();
        List<string> visitedNodes = new List<string>();

        public CheckFlow()
        {
            FillGeometry();
        }

        public bool ConnectionBuffer(double x1, double y1, double a, double b, double x2, double y2)
        {
            bool ans1 = ((Math.Pow(x1 - a, 2) + Math.Pow(y1 - b, 2)) < (ntInfo.snappingTolerance * ntInfo.snappingTolerance));
            bool ans2 = ((Math.Pow(x2 - a, 2) + Math.Pow(y2 - b, 2)) < (ntInfo.snappingTolerance * ntInfo.snappingTolerance));
            return ans1 || ans2;
        }

        public bool NodeBuffer(double x1, double y1, double a, double b)
        {
            bool ans = ((Math.Pow(x1 - a, 2) + Math.Pow(y1 - b, 2)) < (ntInfo.snappingTolerance * ntInfo.snappingTolerance));
            return ans;
        }

        public void AddPointsToPointList(Node dr)
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

        public void FillGeometry()
        {
            foreach (Section dr in _sectionList.Values)
            {
                AddPointsToLineList(dr);
            }

            foreach (Node dr in _nodeList.Values)
            {
                AddPointsToPointList(dr);
            }
        }

        public void AddPointsToLineList(Section dr)
        {
            int xFloor = Convert.ToInt32(Math.Floor(dr.x));
            int xCeiling = Convert.ToInt32(Math.Ceiling(dr.x));

            int yFloor = Convert.ToInt32(Math.Floor(dr.y));
            int yCeiling = Convert.ToInt32(Math.Ceiling(dr.y));

            int toxFloor = Convert.ToInt32(Math.Floor(dr.tox));
            int toxCeiling = Convert.ToInt32(Math.Ceiling(dr.tox));

            int toyFloor = Convert.ToInt32(Math.Floor(dr.toy));
            int toyCeiling = Convert.ToInt32(Math.Ceiling(dr.toy));

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

        public List<INetworkObject> SetFlowFullSystem()
        {
            List<INetworkObject> list = new List<INetworkObject>();
            List<Section> sections = _sectionList.Values.ToList();
            for (int i = 0; i < _sectionList.Values.Count; i++)
            {
                if (StatusExtensions.IsUnenergized(sections[i].status))
                {


                }
            }
            return list;
        }

        public List<Node> GetSnappedNodes(double x, double y)
        {
            List<Node> list = new List<Node>();
            int ToxFloor = Convert.ToInt32(Math.Floor(x));
            int ToyFloor = Convert.ToInt32(Math.Floor(y));
            Point currentPt = new Point(ToxFloor, ToyFloor);
            var nearestPoints = new List<Node>();
            if (pointList.ContainsKey(currentPt)) nearestPoints = pointList[currentPt];
            var snappedNodes = from row in nearestPoints
                               where (NodeBuffer(row.x, row.y, x, y))
                               select row;
            return snappedNodes.ToList();
        }

        public List<Section> GetConnectedEdges(Node node)
        {
            int xFloor = Convert.ToInt32(Math.Floor(node.x));
            int yFloor = Convert.ToInt32(Math.Floor(node.y));
            Point Pt1 = new Point(xFloor, yFloor);
            var nearestSections = new List<Section>();
            if (lineList.ContainsKey(Pt1)) nearestSections = lineList[Pt1];
            var connectedEdges = from row in nearestSections
                                 where (ConnectionBuffer(row.x, row.y, node.x, node.y, row.tox, row.toy))
                                 select row;
            return connectedEdges.ToList();
        }

        public List<Section> GetConnectedSections(Node node)
        {
            List<Section> secs = null;
            if (_nodeList.ContainsKey(node.uid))
            {
                secs = new List<Section>();
                foreach (Section item in node.parentList.Values) secs.Add(item);
                foreach (Section item in node.childList.Values) secs.Add(item);
            }
            return secs;
        }

        public int GetPhaseCodeFromParentSections(Node node)
        {
            int parentPC = 128;
            foreach (Section sec in node.parentList.Values)
            {
                parentPC = PhaseCodeUtil.AddPhaseCodes(parentPC, sec.phaseCode);
            }
            return parentPC;
        }

        public int GetConstructedPhaseCodeFromParentSections(Node node)
        {
            int parentPC = 128;
            foreach (Section sec in node.parentList.Values)
            {
                parentPC = PhaseCodeUtil.AddPhaseCodes(parentPC, sec.constructedPhaseCode);
            }
            return parentPC;
        }

        public int GetPhaseCodeFromChildSections(Node node)
        {
            int childPC = 128;
            foreach (Section sec in node.childList.Values)
            {
                childPC = PhaseCodeUtil.AddPhaseCodes(childPC, sec.phaseCode);
            }
            return childPC;
        }

        public int GetConstructedPhaseCodeFromChildSections(Node node)
        {
            int childPC = 128;
            foreach (Section sec in node.childList.Values)
            {
                childPC = PhaseCodeUtil.AddPhaseCodes(childPC, sec.phaseCode);
            }
            return childPC;
        }

        public Section SelectParentFromParentSections(Node snappedNode)
        {
            foreach (Section sec in snappedNode.parentList.Values)
            {
                if (PhaseCodeUtil.IsPhaseCodePresent(sec.phaseCode, snappedNode.phaseCode))
                    return sec;
            }
            return null;
        }

        public Section SelectParentFromChildSections(Node snappedNode)
        {
            foreach (Section sec in snappedNode.childList.Values)
            {
                if (PhaseCodeUtil.IsPhaseCodePresent(snappedNode.phaseCode, sec.phaseCode))
                    return sec;
            }
            return null;
        }

        public ConnectivityInfo GetParent(double x, double y)
        {
            ConnectivityInfo conInfo = null;
            List<Node> snappedNodes = GetSnappedNodes(x, y);
            Node snappedNode = null;
            if (snappedNodes != null && snappedNodes.Count > 0)
            {
                if (snappedNodes.Count > 1)
                    return null;//snappedNode = snappedNodes.First(nd => StatusExtensions.IsNetJunction(nd.status));
                else
                    snappedNode = snappedNodes[0];
                int phaseCode = 128;
                int constrPhaseCode = 128;
                Section parentSection = null;
                if (snappedNode.parentList.Count > 0)
                {
                    phaseCode = GetPhaseCodeFromParentSections(snappedNode);
                    constrPhaseCode = GetConstructedPhaseCodeFromParentSections(snappedNode);
                    parentSection = SelectParentFromParentSections(snappedNode);
                }
                else
                {
                    phaseCode = GetPhaseCodeFromChildSections(snappedNode);
                    constrPhaseCode = GetConstructedPhaseCodeFromChildSections(snappedNode);
                    parentSection = SelectParentFromChildSections(snappedNode);
                }

                if (parentSection != null)
                {
                    conInfo = new ConnectivityInfo();
                    conInfo.connectedEdges = GetConnectedSections(snappedNode);
                    conInfo.parentFeature = parentSection;
                    conInfo.phaseCode = phaseCode;
                    conInfo.snappedNodes = snappedNodes;
                    conInfo.constructedPhaseCode = constrPhaseCode;
                    if(StatusExtensions.IsEnergized(parentSection.status))
                        conInfo.status = Constants.Energized;
                    if (StatusExtensions.IsUnenergized(parentSection.status))
                        conInfo.status = Constants.Unenergized;
                    if (StatusExtensions.IsLoop(parentSection.status))
                        conInfo.status = Constants.Loop;
                }
            }
            return conInfo;
        }

        public bool ContainsEnergizedUnenergized(Node fromNode, Node toNode)
        {
            bool containsEnergizedUnenergizedSection = false;
            bool energized = false;
            bool unenergized = false;
            foreach (Section sec in fromNode.parentList.Values)
            {
                if (StatusExtensions.IsEnergized(sec.status))
                    energized = true;
                if (StatusExtensions.IsUnenergized(sec.status))
                    unenergized = true;
            }
            foreach (Section sec in fromNode.childList.Values)
            {
                if (StatusExtensions.IsEnergized(sec.status))
                    energized = true;
                if (StatusExtensions.IsUnenergized(sec.status))
                    unenergized = true;
            }
            foreach (Section sec in toNode.parentList.Values)
            {
                if (StatusExtensions.IsEnergized(sec.status))
                    energized = true;
                if (StatusExtensions.IsUnenergized(sec.status))
                    unenergized = true;
            }
            foreach (Section sec in toNode.childList.Values)
            {
                if (StatusExtensions.IsEnergized(sec.status))
                    energized = true;
                if (StatusExtensions.IsUnenergized(sec.status))
                    unenergized = true;
            }
            containsEnergizedUnenergizedSection = energized && unenergized;
            return containsEnergizedUnenergizedSection;
        }

        public bool FromToHasParents(Node fromNode, Node toNode)
        {
            bool fromToParent = false;
            bool fromParent = false;
            bool toParent = false;
            foreach (Section sec in fromNode.parentList.Values)
            {
                fromParent = true;
            }

            foreach (Section sec in toNode.parentList.Values)
            {
                toParent = true;
            }

            fromToParent = fromParent && toParent;
            return fromToParent;
        }

        public bool FromToHasChildren(Node fromNode, Node toNode)
        {
            bool fromToChild = false;
            bool fromChild = false;
            bool toChild = false;
            foreach (Section sec in fromNode.childList.Values)
            {
                fromChild = true;
            }

            foreach (Section sec in toNode.childList.Values)
            {
                toChild = true;
            }

            fromToChild = fromChild && toChild;
            return fromToChild;
        }

        public bool FromToHasNoChildren(Node fromNode, Node toNode)
        {
            bool noFromToChild = true;
            bool noFromChild = true;
            bool noToChild = true;
            foreach (Section sec in fromNode.childList.Values)
            {
                noFromChild = false;
            }

            foreach (Section sec in toNode.childList.Values)
            {
                noToChild = false;
            }

            noFromToChild = noFromChild && noToChild;
            return noFromToChild;
        }

        public bool FromToHasNoParent(Node fromNode, Node toNode)
        {
            bool noFromToParent = true;
            bool noFromParent = true;
            bool noToParent = true;
            foreach (Section sec in fromNode.parentList.Values)
            {
                noFromParent = false;
            }

            foreach (Section sec in toNode.parentList.Values)
            {
                noToParent = false;
            }

            noFromToParent = noFromParent && noToParent;
            return noFromToParent;
        }

        public bool HasParent(Node fromNode)
        {
            bool hasParent = false;
            foreach (Section sec in fromNode.parentList.Values)
            {
                hasParent = true;
            }
            return hasParent;
        }

        public bool HasChild(Node fromNode)
        {
            bool hasChild = false;
            foreach (Section sec in fromNode.parentList.Values)
            {
                hasChild = true;
            }
            return hasChild;
        }

        public bool OneSectionEnergized(Node fromNode)
        {
            bool energized = false;
            foreach (Section sec in fromNode.parentList.Values)
            {
                if (StatusExtensions.IsEnergized(sec.status))
                    energized = true;
            }
            foreach (Section sec in fromNode.childList.Values)
            {
                if (StatusExtensions.IsEnergized(sec.status))
                    energized = true;
            }
            return energized;
        }

        public void FillConnectivityInfoForEnergizedSections(ConnectivityInfo conInfo, Node fromNode, Node toNode)
        {
            conInfo = new ConnectivityInfo();
            conInfo.phaseCode = 128;
            conInfo.parentNode = fromNode;
            conInfo.childNode = toNode;
            conInfo.setFlow = false;
            if (StatusExtensions.IsSource(fromNode.status) || StatusExtensions.IsSource(toNode.status))
            {
                if(StatusExtensions.IsSource(fromNode.status))
                {
                    conInfo.parentNode = fromNode;
                    conInfo.childNode = toNode;
                    conInfo.status = Constants.WithFlow + Constants.Unenergized;
                    conInfo.setFlow = false;
                    conInfo.phaseCode = -1;
                }
                if (StatusExtensions.IsSource(toNode.status))
                {
                    conInfo.parentNode = toNode ;
                    conInfo.childNode = fromNode;
                    conInfo.status = Constants.AgainstFlow + Constants.Unenergized;
                    conInfo.setFlow = false;
                    conInfo.phaseCode = -1;
                }
            }
            else if ((HasParent(fromNode) && !HasChild(fromNode) && HasChild(toNode) && !HasParent(toNode)))
            {
                    conInfo.parentNode = fromNode ;
                    conInfo.childNode = toNode;
                    conInfo.status = Constants.WithFlow ;
                    conInfo.setFlow = false;
            }
            else if ((HasParent(toNode) && !HasChild(toNode) && HasChild(fromNode) && !HasParent(fromNode)))
            {
                    conInfo.parentNode = toNode ;
                    conInfo.childNode = fromNode;
                    conInfo.status = Constants.AgainstFlow;
                    conInfo.setFlow = false;
            }
                else
                {
                    //int fromNodePC = GetPhaseCodeFromParentSections(fromNode);
                    //int toNodePC = GetPhaseCodeFromParentSections(toNode);
                    conInfo.parentNode = fromNode;
                    conInfo.childNode = toNode;
                    conInfo.status = Constants.WithFlow + Constants.Unenergized;
                    conInfo.setFlow = false;
                    conInfo.phaseCode = -1;
                }         
        }

        public bool UnenergizedLocal(Node node)
        {
            if (StatusExtensions.IsDisabled(node.status))
            {
                return true;
            }
            if (StatusExtensions.IsSource(node.status)) return false;
            List<Section> connSections = GetConnectedSections(node);
            bool hasFlowOrLoop = true;
            foreach (Section sec in connSections)
            {
                if (StatusExtensions.IsEnergized(sec.status) || StatusExtensions.IsLoop(sec.status))
                {
                    hasFlowOrLoop = false;
                }
            }
            return hasFlowOrLoop;
        }

        //public void GetNodeStatusToSection(Node nd, Section sec)
        //{
        //    if (StatusExtensions.IsUnenergized(nd.status))
        //    {
        //        netJn.status = Constants.Unenergized + Constants.NetJunction;
        //    }
        //    else if (StatusExtensions.IsEnergized(nd.status))
        //    {
        //        netJn.status = Constants.Energized + Constants.NetJunction;
        //    }
        //    else if (StatusExtensions.IsLoop(nd.status))
        //    {
        //        netJn.status = Constants.Loop + Constants.NetJunction;
        //    }
        //    if (StatusExtensions.IsDisconnected(nd.status))
        //    {
        //        netJn.status = Constants.Disconnected + Constants.NetJunction;
        //    }
        //}

        public ConnectivityInfo GetParent(double x, double y, double tox, double toy)
        {
            ConnectivityInfo conInfo = new ConnectivityInfo();
            List<Node> fromNodeList = GetSnappedNodes(x, y);
            List<Node> toNodeList = GetSnappedNodes(tox, toy);
            Node fromNode = null;
            Node toNode = null;
            if (fromNodeList.Count > 1)
                fromNode = fromNodeList.First(nd => StatusExtensions.IsNetJunction(nd.status));
            else if (fromNodeList != null && fromNodeList.Count!=0)
                fromNode = fromNodeList[0];
            if (toNodeList.Count > 1)
                toNode = toNodeList.First(nd => StatusExtensions.IsNetJunction(nd.status));
            else if(toNodeList!=null && toNodeList.Count!=0)
                toNode = toNodeList[0];
            conInfo.phaseCode = 128;
            conInfo.parentNode = fromNode;
            conInfo.childNode = toNode;
            conInfo.setFlow = false;
            conInfo.status = Constants.Disconnected;

            if (fromNode != null && toNode == null)
            {
                conInfo.status = Constants.WithFlow;
                if(UnenergizedLocal(fromNode)) conInfo.status+= Constants.Unenergized;
                if (!UnenergizedLocal(fromNode)) conInfo.status += Constants.Energized;
                if (StatusExtensions.IsLoop(fromNode.status)) conInfo.status += Constants.Loop;
                if (StatusExtensions.IsDisconnected(fromNode.status)) conInfo.status += Constants.Disconnected;
            }
            if (fromNode == null && toNode != null)
            {
                conInfo.parentNode = toNode;
                conInfo.childNode = fromNode;
                conInfo.status = Constants.AgainstFlow;
                if (UnenergizedLocal(toNode)) conInfo.status += Constants.Unenergized;
                if (!UnenergizedLocal(toNode)) conInfo.status += Constants.Energized;
                if (StatusExtensions.IsLoop(toNode.status)) conInfo.status += Constants.Loop;
                if (StatusExtensions.IsDisconnected(toNode.status)) conInfo.status += Constants.Disconnected;
            }
            if (fromNode != null && toNode != null)
            {
                if (UnenergizedLocal(fromNode) && UnenergizedLocal(toNode))
                {
                    if (StatusExtensions.IsDisconnected(fromNode.status) && StatusExtensions.IsDisconnected(toNode.status))
                    {
                        conInfo.status = Constants.Unenergized + Constants.Disconnected;
                    }
                    else if (!StatusExtensions.IsDisconnected(fromNode.status) && StatusExtensions.IsDisconnected(toNode.status))
                    {
                        conInfo.status = Constants.Unenergized  +Constants.WithFlow;
                        conInfo.setFlow = true;
                        conInfo.phaseCode = -1;
                    }
                    else if (StatusExtensions.IsDisconnected(fromNode.status) && !StatusExtensions.IsDisconnected(toNode.status))
                    {
                        conInfo.parentNode = toNode;
                        conInfo.childNode = fromNode;
                        conInfo.status = Constants.Unenergized + Constants.AgainstFlow;
                        conInfo.setFlow = true;
                        conInfo.phaseCode = -1;
                    }
                    else if (!StatusExtensions.IsDisconnected(fromNode.status) && !StatusExtensions.IsDisconnected(toNode.status))
                    {
                        conInfo.parentNode = fromNode ;
                        conInfo.childNode = toNode;
                        conInfo.status = Constants.Unenergized + Constants.WithFlow;
                        conInfo.phaseCode = -1;
                    }
                }
                if (!UnenergizedLocal(fromNode) && UnenergizedLocal(toNode))
                {
                    conInfo.status = Constants.WithFlow;
                    conInfo.setFlow = true;
                }
                if (UnenergizedLocal(fromNode) && !UnenergizedLocal(toNode))
                {
                    conInfo.parentNode = toNode;
                    conInfo.childNode = fromNode;
                    conInfo.status = Constants.AgainstFlow;
                    conInfo.setFlow = true;
                }
                if (StatusExtensions.IsLoop(fromNode.status) && StatusExtensions.IsLoop(toNode.status))
                {
                    conInfo.status = Constants.Loop;
                }
                else if (!UnenergizedLocal(fromNode) && !UnenergizedLocal(toNode))
                {
                    FillConnectivityInfoForEnergizedSections(conInfo, fromNode, toNode);
                }
            }
            //set phase code
            if (conInfo.parentNode!=null && StatusExtensions.IsNetJunction(conInfo.parentNode.status))
            {
                if (conInfo.phaseCode == -1) conInfo.phaseCode = 128;
                else
                    conInfo.phaseCode = GetPhaseCodeFromParentSections(conInfo.parentNode);
            }
            else
                conInfo.phaseCode = conInfo.parentNode.phaseCode;
            if (conInfo.parentNode != null && StatusExtensions.IsNetJunction(conInfo.parentNode.status))
            {
                conInfo.constructedPhaseCode = GetConstructedPhaseCodeFromParentSections(conInfo.parentNode);
            }
            else
                conInfo.constructedPhaseCode = conInfo.parentNode.constructedPhaseCode;
            return conInfo;
        }



    }
}
