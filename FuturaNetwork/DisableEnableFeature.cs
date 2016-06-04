using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuturaNetwork
{
    public class DisableEnableFeature
    {
        public static void Disable(Node nd)
        {
            if (ConnectedSectionsInLoop(nd))
            {
                Node strtNode = GetCommonLoopParent(nd);
            }
        }

        public static void FlipSection(Section sec)
        {
            Node parentNode = sec.parentNode;
            Node childNode = sec.childNode;
            sec.childNode = parentNode;
            sec.parentNode = childNode;
            childNode.parentList.Remove(sec.uid);
            childNode.childList.Add(sec.uid, sec);
        }

        public static void DownsectionPCCorrection(Node parentNode, Section childSection)
        {
            int parentPCAccumilated = 128;
            int parentPC = 128;
            int parentConstrPC = parentNode.constructedPhaseCode;
            int constrChildPC = childSection.constructedPhaseCode;
            foreach (Section sec in parentNode.parentList.Values)
            {
                parentPCAccumilated = PhaseCodeUtil.AddPhaseCodes(parentPC, sec.phaseCode);
            }
            if (!StatusExtensions.IsNetJunction(parentNode.status))
            {
                if (PhaseCodeUtil.IsPhaseCodePresent(parentConstrPC, parentPCAccumilated)) // may add phase here
                {
                    if (!StatusExtensions.IsYDTransformer(parentNode.status))
                    {
                        parentNode.phaseCode = parentPCAccumilated;
                    }
                }
                else if (PhaseCodeUtil.IsPhaseCodePresent(parentPCAccumilated, parentConstrPC))
                {
                    parentNode.phaseCode = parentConstrPC;
                }
                else if (!PhaseCodeUtil.HaveCommonPhaseCodes(parentPCAccumilated, parentConstrPC))
                {
                    parentNode.phaseCode = 128;
                }
            }
            if (StatusExtensions.IsNetJunction(parentNode.status))
            {
                parentPC = parentPCAccumilated;
            }
            else
                parentPC = parentNode.phaseCode;
            int existingChildPC = childSection.phaseCode;
            if (PhaseCodeUtil.IsPhaseCodePresent(constrChildPC, parentPC)) // may add phase here
            {
                childSection.phaseCode = parentPC;
            }
            else if (PhaseCodeUtil.IsPhaseCodePresent(parentPC, constrChildPC))
            {
                childSection.phaseCode = constrChildPC;
            }
            else if (!PhaseCodeUtil.HaveCommonPhaseCodes(parentPC, constrChildPC))
            {
                childSection.phaseCode = 128;
            }
            if (childSection.phaseCode != 128)
            {
                if (StatusExtensions.IsUnenergized(childSection.status))
                    StatusExtensions.Energize(childSection.status);                

            }
            else
            {
                if (!StatusExtensions.IsUnenergized(childSection.status))
                {
                    StatusExtensions.Unenerize(childSection.status);
                    childSection.status += Constants.Unenergized;
                }
            }
            if (StatusExtensions.IsDisconnected(childSection.status))
                StatusExtensions.Connect(childSection.status);
        }

        public static void FlipDigitized(Section sec)
        {
            if (StatusExtensions.IsAgainstFlow(sec.status)) sec.status = Constants.WithFlow;
            else if (StatusExtensions.IsWithFlow(sec.status)) sec.status = Constants.AgainstFlow;
        }

        public static void SetFlowUnenergize(Node nd, HashSet<Section> visited)
        {
            List<Node> disabledList = new List<Node>();
            if (nd != null)
            {
                bool first = true;
                Queue<Node> q = new Queue<Node>();
                q.Enqueue(nd);
                while (q.Count > 0)
                {
                    Node current = q.Dequeue();
                    StatusExtensions.UnLoop(current.status);
                    if (current == null)
                        continue;
                    if (StatusExtensions.IsDisabled(current.status) && first)
                    {
                        current.phaseCode = 128;
                    }
                    else if (StatusExtensions.IsDisabled(current.status) && !first)
                    {
                        disabledList.Add(current);
                        continue;
                    }
                    //downstream
                    foreach (var item in current.childList.Values)
                    {
                        if (!visited.Contains(item))
                        {
                            visited.Add(item);
                            q.Enqueue(item.childNode);
                            NetworkUtil.DownsectionPCAdjustment(current, item);
                        }
                    }
                }
            }
            for (int i = 0; i < disabledList.Count; i++)
            {
                SetFlowUnenergize(disabledList[i], visited);
            }
        }

        public static void SetFlow(Node nd)
        {
            HashSet<Section> visited = new HashSet<Section>();
            SetFlowEnergize(nd, visited);
        }

        public static void SetFlowEnergize(Node nd, HashSet<Section> visited)
        {
            List<Node> disabledList = new List<Node>();
            if (nd != null)
            {
                Queue<Node> q = new Queue<Node>();
                q.Enqueue(nd);
                while (q.Count > 0)
                {
                    Node current = q.Dequeue();
                    //StatusExtensions.UnLoop(current.status);
                    if (current == null)
                        continue;
                    StatusExtensions.Energize(current.status);
                    StatusExtensions.Connect(current.status);
                    if (StatusExtensions.IsLoop(current.status))
                    {
                        Node strtNode = GetCommonLoopParent(nd);
                        SetFlowEnergizeLoop(strtNode,visited);
                    }
                    if (StatusExtensions.IsDisabled(current.status))
                    {
                        disabledList.Add(current);
                    }
                    else
                    {
                        Dictionary<string, Section> connSecs = GetConnectedUnenergizedSections(current, visited);
                        foreach (var item in connSecs.Values)
                        {
                            if (!visited.Contains(item))
                            {
                                visited.Add(item);
                                StatusExtensions.UnLoop(item.status);
                                if (current == item.childNode)
                                {
                                    FlipSection(item);
                                    FlipDigitized(item);
                                    q.Enqueue(item.childNode);
                                }
                                q.Enqueue(item.childNode);
                                DownsectionPCCorrection(current, item);
                            }
                        }
                        //downstream
                        foreach (var item in current.childList.Values)
                        {
                            if (!visited.Contains(item) && !connSecs.ContainsKey(item.uid))
                            {
                                visited.Add(item);
                                q.Enqueue(item.childNode);
                                DownsectionPCCorrection(current, item);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < disabledList.Count; i++)
            {
                SetFlowUnenergize(disabledList[i], visited);
            }
        }

        public static void SetFlowEnergizeLoop(Node nd, HashSet<Section> visited)
        {
            List<Node> disabledList = new List<Node>();
            if (nd != null)
            {
                Queue<Node> q = new Queue<Node>();
                q.Enqueue(nd);
                while (q.Count > 0)
                {
                    Node current = q.Dequeue();
                    StatusExtensions.UnLoop(current.status);
                    if (current == null)
                        continue;
                    if (StatusExtensions.IsDisabled(current.status))
                    {
                        disabledList.Add(current);
                    }
                    else
                    {
                        Dictionary<string, Section> connSecs = GetConnectedLoopSections(current, visited);
                        foreach (var item in connSecs.Values)
                        {
                            if (!visited.Contains(item) && StatusExtensions.IsLoop(item.status))
                            {
                                visited.Add(item);
                                StatusExtensions.UnLoop(item.status);
                                if (current == item.childNode)
                                {
                                    FlipSection(item);
                                    FlipDigitized(item);
                                    q.Enqueue(item.childNode);
                                }
                                q.Enqueue(item.childNode);
                                NetworkUtil.DownsectionPCAdjustment(current, item);
                            }
                        }
                        //downstream
                        foreach (var item in current.childList.Values)
                        {
                            if (!visited.Contains(item) && !connSecs.ContainsKey(item.uid))
                            {
                                visited.Add(item);
                                q.Enqueue(item.childNode);
                                NetworkUtil.DownsectionPCAdjustment(current, item);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < disabledList.Count; i++)
            {
                SetFlowUnenergize(disabledList[i], visited);
            }
        }

        public static Dictionary<string, Section> GetConnectedLoopSections(Node nd, HashSet<Section> visited)
        {
            Dictionary<string, Section> connSecs = new Dictionary<string, Section>();
            foreach (string secUID in nd.parentList.Keys)
            {
                if (StatusExtensions.IsLoop(nd.parentList[secUID].status) && !visited.Contains(nd.parentList[secUID]))
                    connSecs.Add(secUID, nd.parentList[secUID]);
            }
            foreach (string secUID in nd.childList.Keys)
            {
                if (StatusExtensions.IsLoop(nd.childList[secUID].status) && !visited.Contains(nd.childList[secUID]))
                    connSecs.Add(secUID, nd.childList[secUID]);
            }
            return connSecs;
        }

        public static Dictionary<string, Section> GetConnectedUnenergizedSections(Node nd, HashSet<Section> visited)
        {
            Dictionary<string, Section> connSecs = new Dictionary<string, Section>();
            foreach (string secUID in nd.parentList.Keys)
            {
                if ((StatusExtensions.IsDisconnected(nd.parentList[secUID].status) || StatusExtensions.IsUnenergized(nd.parentList[secUID].status)) && !visited.Contains(nd.parentList[secUID]))
                    connSecs.Add(secUID, nd.parentList[secUID]);
            }
            foreach (string secUID in nd.childList.Keys)
            {
                if ((StatusExtensions.IsDisconnected(nd.parentList[secUID].status) || StatusExtensions.IsUnenergized(nd.parentList[secUID].status)) && !visited.Contains(nd.childList[secUID]))
                    connSecs.Add(secUID, nd.childList[secUID]);
            }
            return connSecs;
        }

        public static List<string> GuidsToRoot(Node leaf, HashSet<Section> visited)
        {
            List<string> path = new List<string>();
            if (leaf != null)
            {
                Queue<Node> q = new Queue<Node>();
                q.Enqueue(leaf);
                while (q.Count > 0)
                {
                    Node current = q.Dequeue();
                    if (current == null)
                        continue;
                    path.Add(current.uid);
                    Dictionary<string, Section> connSecs = GetConnectedLoopSections(current, visited);
                    foreach (var item in connSecs.Values)
                    {
                        if (!visited.Contains(item))
                        {
                            visited.Add(item);
                            if (current == item.parentNode) q.Enqueue(item.childNode);
                            if (current == item.childNode) q.Enqueue(item.parentNode);
                            //path.Add(item.uid);
                            break;
                        }
                    }
                }
            }
            return path;
        }

        public static Node GetCommonLoopParent(Node nd)
        {
            Node intersectionNode = null;
            HashSet<Section> visited = new HashSet<Section>();
            List<Section> connSecs = GetConnectedLoopSections(nd, visited).Values.ToList();
            string intersectionPt = "";
            for (int i = 0; i < connSecs.Count - 1; i++)
            {
                for (int j = i + 1; j < connSecs.Count; j++)
                {
                    Section item1 = connSecs[i];
                    Section item2 = connSecs[j];
                    List<string> lst1 = new List<string>();
                    List<string> lst2 = new List<string>();
                    visited.Add(connSecs[i]);
                    visited.Add(connSecs[j]);
                    if (nd == item1.parentNode) lst1 = GuidsToRoot(item1.childNode, visited);
                    else if (nd == item1.childNode) lst1 = GuidsToRoot(item1.parentNode, visited);
                    if (nd == item2.parentNode) lst2 = GuidsToRoot(item2.childNode, visited);
                    else if (nd == item2.childNode) lst2 = GuidsToRoot(item2.parentNode, visited);
                    List<string> intersections = lst1.Intersect(lst2).ToList<string>();
                    if (intersections.Count != 0)
                    {
                        intersectionPt = intersections[0];
                        int i1 = lst1.IndexOf(intersectionPt);
                        int i2 = lst2.IndexOf(intersectionPt);
                    }
                    if (StaticStuff._nodeList.ContainsKey(intersectionPt))
                        intersectionNode = StaticStuff._nodeList[intersectionPt];
                    if (intersectionNode != null) break;
                }
            }
            return intersectionNode;
        }

        //public static Node EnergizedNodeFromLoop(Node nd)
        //{
        //    Queue<Node> q = new Queue<Node>();
        //    q.Enqueue(root);
        //    if (root != null)
        //    {
        //        while (q.Count > 0)
        //        {
        //            Node current = q.Dequeue();
        //            if (current != null && !StatusExtensions.IsNetJunction(current.status))
        //                list.Add(current); //add node
        //            if (current == null || StatusExtensions.IsDisconnected(current.status))
        //                continue;
        //            if (ContainsAdjNode(current))
        //            {
        //                q.Enqueue(current.adjacentNode);
        //            }
        //            parentPhase = current.phaseCode;
        //            if (StatusExtensions.IsUnenergized(current.status) || StatusExtensions.IsDisabled(current.status))
        //            {
        //                foreach (var item in current.childList.Values)
        //                {

        //                    q.Enqueue(item.childNode);

        //                }
        //                foreach (var item in current.parentList.Values)
        //                {

        //                    q.Enqueue(item.childNode);

        //                }
        //            }
        //            else
        //            {
        //                foreach (var item in current.childList.Values)
        //                {

        //                    q.Enqueue(item.childNode);

        //                }
        //            }
        //        }
        //    }
        //}

        public static bool ConnectedSectionsInLoop(Node nd)
        {
            bool inLoop = false;
            foreach (Section sec in nd.parentList.Values)
            {
                if (StatusExtensions.IsLoop(sec.status))
                    inLoop = true;
            }
            foreach (Section sec in nd.childList.Values)
            {
                if (StatusExtensions.IsLoop(sec.status))
                    inLoop = true;
            }
            return inLoop;
        }

    }
}
