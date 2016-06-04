using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;
using UndoRedoFramework.Collections.Generic;
using UndoRedoFramework;

namespace FuturaNetwork
{

    public class NetworkUtil
    {

        public class Pair<T1, T2>
        {
            public T1 First { get; set; }
            public T2 Second { get; set; }
        }
        public static Dictionary<string, Node> _nodeList = StaticStuff._nodeList;
        public static Dictionary<string, Section> _sectionList = StaticStuff._sectionList;
        public static DBUtil dbutil = StaticStuff.dbutil;
        public NTInfo ntInfo = StaticStuff.ntInfo;
        public Dictionary<int, int> primaryStatuses = new Dictionary<int, int>();
        public Dictionary<int, int> secondaryStatuses = new Dictionary<int, int>();
        List<string> visitedNodes = new List<string>();
        CreateNetwork crNet = new CreateNetwork();

        public bool IsConnectionEnergized(Node nd)
        {
            foreach (Section sec in nd.parentList.Values)
            {
                if (StatusExtensions.IsEnergized(sec.status))
                    return true;
            }
            return false;
        }

        public void PopulateConductors()
        {
            foreach (Section sec in _sectionList.Values)
            {
                try
                {
                    if (sec != null)
                    {
                        if (sec.classID == ntInfo.networkClassIds["PrimaryConductor"]) primaryStatuses.Add(sec.objectId, sec.status);
                        if (sec.classID == ntInfo.networkClassIds["SecondaryConductor"]) secondaryStatuses.Add(sec.objectId, sec.status);
                    }
                }
                catch (Exception exc)
                {
                }

            }
        }

        public void LoadData()
        {
            StaticStuff.InitializeNTInfo();
            StaticStuff.FillElementsFromDB();
            DataTable dtConn = dbutil.GetConnectivity();
            foreach (DataRow dr in dtConn.Rows)
            {
                Section sect = new Section();
                Node pnd = null;
                Node cnd = null;
                int SessionId = (dr["SessionId"] == null || dr["SessionId"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["SessionId"]);
                string sectUID = (dr["UID"] == null || dr["UID"] == DBNull.Value) ? "" : dr["UID"].ToString();
                sect.uid = sectUID;
                string Parent = (dr["ParentNode"] == null || dr["ParentNode"] == DBNull.Value) ? "" : dr["ParentNode"].ToString();
                string Child = (dr["ChildNode"] == null || dr["ChildNode"] == DBNull.Value) ? "" : dr["ChildNode"].ToString();
                if (!string.IsNullOrEmpty(sectUID) && !string.IsNullOrEmpty(Parent) && !string.IsNullOrEmpty(Child))
                {
                    if (_sectionList.ContainsKey(sectUID))
                    {
                        sect = _sectionList[sectUID];
                    }
                    else
                    {
                    }
                    sect.phaseCode = (dr["PhaseCode"] == null || dr["PhaseCode"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["PhaseCode"]);
                    sect.status = ((dr["status"] == null || dr["status"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["status"]));

                    if (_nodeList.ContainsKey(Parent))
                    {
                        pnd = _nodeList[Parent];
                        pnd.phaseCode = (dr["PNPhaseCode"] == null || dr["PNPhaseCode"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["PNPhaseCode"]);
                        pnd.status = ((dr["PNStatus"] == null || dr["PNStatus"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["PNStatus"]));
                    }
                    else if (!string.IsNullOrEmpty(Parent))
                    {
                        int PNPhaseCode = (dr["PNPhaseCode"] == null || dr["PNPhaseCode"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["PNPhaseCode"]);
                        int PNStatus = (dr["PNStatus"] == null || dr["PNStatus"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["PNStatus"]);
                        pnd = new Node(Parent, PNStatus, PNPhaseCode);
                        _nodeList.Add(Parent, pnd);
                    }
                    else
                    {
                    }

                    if (_nodeList.ContainsKey(Child))
                    {
                        cnd = _nodeList[Child];
                        cnd.phaseCode = (dr["CNPhaseCode"] == null || dr["CNPhaseCode"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["CNPhaseCode"]);
                        cnd.status = ((dr["CNStatus"] == null || dr["CNStatus"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["CNStatus"]));
                    }
                    else if (!string.IsNullOrEmpty(Child))
                    {
                        int CNPhaseCode = (dr["CNPhaseCode"] == null || dr["CNPhaseCode"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["CNPhaseCode"]);
                        int CNStatus = ((dr["CNStatus"] == null || dr["CNStatus"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["CNStatus"]));
                        cnd = new Node(Child, CNStatus, CNPhaseCode);
                        _nodeList.Add(Child, cnd);
                    }
                    sect.parentNode = pnd;
                    sect.childNode = cnd;
                    if (!_sectionList.ContainsKey(sect.uid))
                    {
                        _sectionList.Add(sect.uid, sect);
                    }

                    //check pn child list
                    if (!pnd.childList.ContainsKey(sect.uid))
                    {
                        pnd.childList.Add(sect.uid, sect);
                    }

                    //check cn parent list
                    if (!cnd.parentList.ContainsKey(sect.uid))
                    {
                        cnd.parentList.Add(sect.uid, sect);
                    }
                    if (ContainsAdjNode(cnd))
                    {
                        if (!cnd.adjacentNode.parentList.ContainsKey(sect.uid))
                        {
                            cnd.adjacentNode.parentList.Add(sect.uid, sect);
                        }
                    }
                }
            }
            //UpdateConnectivity();
        }

        public void UndoRedoTest()
        {
            UndoRedoDictionary<string, Section> sectURList = _sectionList as UndoRedoDictionary<string, Section>;

            using (UndoRedoManager.Start("Edit "))
            {
                sectURList.Add("0", new Section());
                sectURList.Add("1", new Section());
                UndoRedoManager.Commit();
            }

            using (UndoRedoManager.Start("Edit "))
            {
                sectURList.Remove("0");
                //sectURList.Add("0", new Section());
                sectURList.Add("2", new Section());
                UndoRedoManager.Commit();
            }
            UndoRedoManager.Undo();


            UndoRedoManager.Redo();
            using (UndoRedoManager.Start(""))
            {
                foreach (Section sec in sectURList.Values)
                {

                    if (sec.status == 16)
                    {
                        sec.statusUR.SetVal(67);

                    }
                }
                UndoRedoManager.Commit();
            }



            using (UndoRedoManager.Start("Edit "))
            {
                foreach (Section sec in sectURList.Values)
                {

                    if (sec.status == 33)
                    {
                        sec.statusUR.SetVal(43);

                    }
                }
                UndoRedoManager.Commit();
            }

            UndoRedoManager.Undo();

            foreach (Section sec in sectURList.Values)
            {

                if (sec.status == 67)
                {
                }
                if (sec.status == 43)
                {
                }
            }
            UndoRedoManager.Undo();
            foreach (Section sec in sectURList.Values)
            {

                if (sec.status == 67)
                {
                }
                if (sec.status == 43)
                {
                }
            }


            UndoRedoManager.Redo();
            foreach (Section sec in sectURList.Values)
            {

                if (sec.status == 67)
                {
                }
                if (sec.status == 43)
                {
                }
            }
            UndoRedoManager.Redo();
            foreach (Section sec in sectURList.Values)
            {

                if (sec.status == 67)
                {
                }
                if (sec.status == 43)
                {
                }
            }

        }

        public List<INetworkObject> DownStream(string strtGuid, int type, int searchPhase, string stopGuid)
        {
            List<INetworkObject> list = new List<INetworkObject>();
            HashSet<INetworkObject> sectionSet = new HashSet<INetworkObject>();
            INetworkObject stopPoint = null;
            if (_nodeList.ContainsKey(stopGuid)) stopPoint = _nodeList[stopGuid];
            if (_sectionList.ContainsKey(stopGuid)) stopPoint = _sectionList[stopGuid];
            int parentPhase = PhaseCodeBitgateMapping.ABC_BitgateValue;
            bool specifiedPhase = false;
            Node root = null;
            if (searchPhase != -1) specifiedPhase = true;
            if (type == 0 && _nodeList.ContainsKey(strtGuid))
            {
                root = _nodeList[strtGuid];
                parentPhase = root.phaseCode;
            }
            else if (type == 1 && _sectionList.ContainsKey(strtGuid))
            {
                Section secRoot = _sectionList[strtGuid];
                if (secRoot != null && !StatusExtensions.IsUnenergized(secRoot.status) && !StatusExtensions.IsDisconnected(secRoot.status) && !StatusExtensions.IsLoop(secRoot.status))
                {
                    parentPhase = secRoot.phaseCode;
                    list.Add(secRoot);
                    sectionSet.Add(secRoot);
                    if (secRoot.childNode != null && DownsectionPCCompatibleWithSection(secRoot, secRoot.childNode, searchPhase, specifiedPhase))
                        root = secRoot.childNode;
                }
            }

            Queue<Node> q = new Queue<Node>();
            q.Enqueue(root);
            if (root != null)
            {
                list.Add(root);
                while (q.Count > 0)
                {
                    Node current = q.Dequeue();
                    if (current != null && !StatusExtensions.IsNetJunction(current.status))
                        list.Add(current); //add node
                    if (current == null || StatusExtensions.IsDisabled(current.status) || StatusExtensions.IsUnenergized(current.status) || StatusExtensions.IsDisconnected(current.status))
                        continue;
                    parentPhase = current.phaseCode;
                    foreach (var item in current.childList.Values)
                    {
                        if (stopPoint != null && (item == stopPoint || item.childNode == stopPoint)) continue;
                        if (!sectionSet.Contains(item) && !StatusExtensions.IsUnenergized(item.status) && !StatusExtensions.IsDisconnected(item.status) && !StatusExtensions.IsLoop(current.status) && DownsectionPCCompatible(current, item.phaseCode, searchPhase, specifiedPhase))
                        {
                            q.Enqueue(item.childNode);
                            sectionSet.Add(item);
                            if (item != null)
                                list.Add(item); //add section                            
                        }
                    }
                }
            }
            return list;
        }

        public List<INetworkObject> FindLoops()
        {
            List<INetworkObject> list = new List<INetworkObject>();
            foreach (Section sec in _sectionList.Values)
            {
                if (StatusExtensions.IsLoop(sec.status))
                {
                    list.Add(sec);
                }
            }
            return list;
        }

        public List<INetworkObject> FindDisconnected()
        {
            List<INetworkObject> list = new List<INetworkObject>();
            foreach (Section sec in _sectionList.Values)
            {
                if (StatusExtensions.IsDisconnected(sec.status))
                {
                    list.Add(sec);
                }
            }
            foreach (Node nd in _nodeList.Values)
            {
                if (StatusExtensions.IsDisconnected(nd.status))
                {
                    list.Add(nd);
                }
            }
            return list;
        }

        public static bool ContainsAdjNode(Node nd)
        {
            if (nd != null && nd.adjacentNode != null && !string.IsNullOrEmpty(nd.adjacentNode.uid))
                return true;
            return false;
        }

        public List<INetworkObject> UpStream(string strtGuid, int type, int searchPhase, string stopGuid)
        {
            List<INetworkObject> list = new List<INetworkObject>();
            HashSet<INetworkObject> sectionSet = new HashSet<INetworkObject>();
            INetworkObject stopPoint = null;
            if (_nodeList.ContainsKey(stopGuid)) stopPoint = _nodeList[stopGuid];
            if (_sectionList.ContainsKey(stopGuid)) stopPoint = _sectionList[stopGuid];
            int childPhase = PhaseCodeBitgateMapping.Unknown_BitgateValue;
            bool specifiedPhase = false;
            Pair<int, Node> root = null;
            if (searchPhase != -1) specifiedPhase = true;
            if (type == 0 && _nodeList.ContainsKey(strtGuid))
            {
                root = new Pair<int, Node>();
                root.First = childPhase;
                root.Second = _nodeList[strtGuid];
                //childPhase = root.Second.phaseCode;
            }
            else if (type == 1 && _sectionList.ContainsKey(strtGuid))
            {
                Section secRoot = _sectionList[strtGuid];
                if (secRoot != null && !StatusExtensions.IsUnenergized(secRoot.status) && !StatusExtensions.IsDisconnected(secRoot.status) && !StatusExtensions.IsLoop(secRoot.status))
                {
                    childPhase = secRoot.phaseCode;
                    list.Add(secRoot);
                    sectionSet.Add(secRoot);
                    if (secRoot.parentNode != null)
                    {
                        root = new Pair<int, Node>();
                        root.First = childPhase;
                        root.Second = secRoot.parentNode;
                    }
                }
            }

            Queue<Pair<int, Node>> q = new Queue<Pair<int, Node>>();
            q.Enqueue(root);
            if (root != null)
            {
                list.Add(root.Second);
                while (q.Count > 0)
                {
                    Pair<int, Node> currentTuple = q.Dequeue();
                    Node current = currentTuple.Second;
                    childPhase = currentTuple.First;
                    if (ContainsAdjNode(current))
                    {
                        Pair<int, Node> adjTuple = new Pair<int, Node>();
                        adjTuple.First = childPhase;
                        adjTuple.Second = current.adjacentNode;
                        q.Enqueue(adjTuple);
                    }
                    if (current != null && !StatusExtensions.IsNetJunction(current.status))
                        list.Add(current);
                    if (current == null || StatusExtensions.IsDisabled(current.status) || StatusExtensions.IsUnenergized(current.status) || StatusExtensions.IsDisconnected(current.status))
                        continue;
                    foreach (var item in current.parentList.Values)
                    {
                        if (stopPoint != null && (item == stopPoint || item.childNode == stopPoint)) continue;
                        if (!sectionSet.Contains(item) && !StatusExtensions.IsUnenergized(item.status) && !StatusExtensions.IsDisconnected(item.status) && !StatusExtensions.IsLoop(item.status) && UpsectionPCCompatible(childPhase, current, item.phaseCode, searchPhase, specifiedPhase))
                        {
                            Pair<int, Node> nextTuple = new Pair<int, Node>();
                            nextTuple.First = item.phaseCode;
                            nextTuple.Second = item.parentNode;
                            q.Enqueue(nextTuple);
                            sectionSet.Add(item);
                            if (item != null)
                                list.Add(item);
                        }
                    }
                }
            }
            return list;
        }

        public List<INetworkObject> DownStreamAll(string strtGuid, int type, int searchPhase, string stopGuid)
        {
            List<INetworkObject> list = new List<INetworkObject>();
            HashSet<INetworkObject> sectionSet = new HashSet<INetworkObject>();
            INetworkObject stopPoint = null;
            if (_nodeList.ContainsKey(stopGuid)) stopPoint = _nodeList[stopGuid];
            if (_sectionList.ContainsKey(stopGuid)) stopPoint = _sectionList[stopGuid];
            int parentPhase = PhaseCodeBitgateMapping.ABC_BitgateValue;
            bool specifiedPhase = false;
            Node root = null;
            if (searchPhase != -1) specifiedPhase = true;
            if (type == 0 && _nodeList.ContainsKey(strtGuid))
            {
                root = _nodeList[strtGuid];
                parentPhase = root.phaseCode;
            }
            else if (type == 1 && _sectionList.ContainsKey(strtGuid))
            {
                Section secRoot = _sectionList[strtGuid];
                if (secRoot != null && !StatusExtensions.IsUnenergized(secRoot.status) && !StatusExtensions.IsDisconnected(secRoot.status) && !StatusExtensions.IsLoop(secRoot.status))
                {
                    parentPhase = secRoot.phaseCode;
                    list.Add(secRoot);
                    sectionSet.Add(secRoot);
                    if (secRoot.childNode != null && DownsectionPCCompatibleWithSection(secRoot, secRoot.childNode, searchPhase, specifiedPhase))
                        root = secRoot.childNode;
                }
            }

            Queue<Node> q = new Queue<Node>();
            q.Enqueue(root);
            if (root != null)
            {
                list.Add(root);
                while (q.Count > 0)
                {
                    Node current = q.Dequeue();
                    if (current != null && !StatusExtensions.IsNetJunction(current.status))
                        list.Add(current); //add node
                    if (current == null || StatusExtensions.IsDisconnected(current.status))
                        continue;
                    if (ContainsAdjNode(current))
                    {
                        q.Enqueue(current.adjacentNode);
                    }
                    parentPhase = current.phaseCode;
                    if (StatusExtensions.IsUnenergized(current.status) || StatusExtensions.IsDisabled(current.status))
                    {
                        foreach (var item in current.childList.Values)
                        {
                            if (stopPoint != null && (item == stopPoint || item.childNode == stopPoint)) continue;
                            if (!sectionSet.Contains(item) && StatusExtensions.IsUnenergized(item.status) && DownsectionPCCompatible(current, item.phaseCode, searchPhase, specifiedPhase))
                            {
                                q.Enqueue(item.childNode);
                                sectionSet.Add(item);
                                list.Add(item); //add section                            
                            }
                        }
                        foreach (var item in current.parentList.Values)
                        {
                            if (stopPoint != null && (item == stopPoint || item.childNode == stopPoint)) continue;
                            if (!sectionSet.Contains(item) && StatusExtensions.IsUnenergized(item.status) && DownsectionPCCompatible(current, item.phaseCode, searchPhase, specifiedPhase))
                            {
                                q.Enqueue(item.childNode);
                                sectionSet.Add(item);
                                list.Add(item); //add section                            
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in current.childList.Values)
                        {
                            if (stopPoint != null && (item == stopPoint || item.childNode == stopPoint)) continue;
                            if (!sectionSet.Contains(item) && !StatusExtensions.IsDisconnected(item.status) && !StatusExtensions.IsLoop(current.status) && DownsectionPCCompatible(current, item.phaseCode, searchPhase, specifiedPhase))
                            {
                                q.Enqueue(item.childNode);
                                sectionSet.Add(item);
                                list.Add(item); //add section                            
                            }
                        }
                    }
                }
            }
            return list;
        }

        public static bool UpsectionPCCompatible(int childPC, Node childNode, int parentPC, int searchPhase, bool specifiedPhase)
        {
            int childNodePC = 0;
            if (StatusExtensions.IsNetJunction(childNode.status))
            {
                childNodePC = childPC;
            }
            else
            {
                childNodePC = childNode.phaseCode;
            }
            if (specifiedPhase)
            {
                if (StatusExtensions.IsYDTransformer(childNode.status))
                {
                    if (PhaseCodeUtil.IsOnePhaseCodeDifferent(parentPC, childNodePC) && PhaseCodeUtil.IsPhaseCodePresent(childNodePC, searchPhase) && PhaseCodeUtil.IsPhaseCodePresent(parentPC, searchPhase))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (PhaseCodeUtil.IsPhaseCodePresent(parentPC, childNodePC) && PhaseCodeUtil.IsPhaseCodePresent(parentPC, searchPhase))
                    return true;
                else
                    return false;
            }
            else
            {
                if (StatusExtensions.IsYDTransformer(childNode.status))
                {
                    if (PhaseCodeUtil.IsOnePhaseCodeDifferent(parentPC, childNodePC))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (PhaseCodeUtil.IsPhaseCodePresent(parentPC, childNodePC))
                    return true;
                else return false;
            }
        }

        public static int GetParentPCSum(Node node)
        {
            int parentPC = 128;
            foreach (Section sec in node.parentList.Values)
            {
                parentPC = PhaseCodeUtil.AddPhaseCodes(parentPC, sec.phaseCode);
            }
            return parentPC;
        }

        public static int GetChildPCSum(Node node)
        {
            int childPC = 128;
            foreach (Section sec in node.childList.Values)
            {
                childPC = PhaseCodeUtil.AddPhaseCodes(childPC, sec.phaseCode);
            }
            return childPC;
        }

        public static bool DownsectionPCCompatible(Node parentNode, int childPC, int searchPhase, bool specifiedPhase)
        {
            int parentPC = 128;
            if (StatusExtensions.IsNetJunction(parentNode.status))
            {
                foreach (Section sec in parentNode.parentList.Values)
                {
                    parentPC = PhaseCodeUtil.AddPhaseCodes(parentPC, sec.phaseCode);
                }
            }
            else
            {
                parentPC = parentNode.phaseCode;
            }
            if (specifiedPhase)
            {
                if (StatusExtensions.IsYDTransformer(parentNode.status))
                {
                    if (PhaseCodeUtil.IsOnePhaseCodeDifferent(parentPC, childPC) && PhaseCodeUtil.IsPhaseCodePresent(childPC, searchPhase) && PhaseCodeUtil.IsPhaseCodePresent(parentPC, searchPhase))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (PhaseCodeUtil.IsPhaseCodePresent(parentPC, childPC) && PhaseCodeUtil.IsPhaseCodePresent(childPC, searchPhase))
                    return true;
                else
                    return false;
            }
            else
            {
                if (StatusExtensions.IsYDTransformer(parentNode.status))
                {
                    if (PhaseCodeUtil.IsOnePhaseCodeDifferent(parentPC, childPC))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (PhaseCodeUtil.IsPhaseCodePresent(parentPC, childPC))
                    return true;
                else return false;
            }
        }

        public static bool VerifyNodePCWithParents(Node node, int searchPhase, bool specifiedPhase)
        {
            int parentPC = 128;
            int childPC = node.phaseCode;
            if (StatusExtensions.IsNetJunction(node.status))
            {
                return true;
            }
            else
            {
                foreach (Section sec in node.parentList.Values)
                {
                    parentPC = PhaseCodeUtil.AddPhaseCodes(parentPC, sec.phaseCode);
                }
            }
            if (specifiedPhase)
            {
                if (StatusExtensions.IsYDTransformer(node.status))
                {
                    if (PhaseCodeUtil.IsOnePhaseCodeDifferent(parentPC, childPC) && PhaseCodeUtil.IsPhaseCodePresent(childPC, searchPhase) && PhaseCodeUtil.IsPhaseCodePresent(parentPC, searchPhase))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (PhaseCodeUtil.IsPhaseCodePresent(parentPC, childPC) && PhaseCodeUtil.IsPhaseCodePresent(childPC, searchPhase))
                    return true;
                else
                    return false;
            }
            else
            {
                if (StatusExtensions.IsYDTransformer(node.status))
                {
                    if (PhaseCodeUtil.IsOnePhaseCodeDifferent(parentPC, childPC))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (PhaseCodeUtil.IsPhaseCodePresent(parentPC, childPC))
                    return true;
                else return false;
            }
        }

        public static void DownsectionPCAdjustment(Node parentNode, Section childSection)
        {
            int parentPC = 128;
            if (StatusExtensions.IsNetJunction(parentNode.status))
            {
                foreach (Section sec in parentNode.parentList.Values)
                {
                    parentPC = PhaseCodeUtil.AddPhaseCodes(parentPC, sec.phaseCode);
                }
            }
            else
            {
                parentPC = parentNode.phaseCode;
            }
            int existingChildPC = childSection.phaseCode;
            bool update = true;
            if (PhaseCodeUtil.IsPhaseCodePresent(existingChildPC, parentPC))
            {
                if (StatusExtensions.IsYDTransformer(parentNode.status))
                {
                    if (!PhaseCodeUtil.IsOnePhaseCodeDifferent(parentPC, existingChildPC))
                    {
                        update = false;
                    }
                }
                if (update)
                {
                    childSection.phaseCode = parentPC;
                    StatusExtensions.Energize(childSection.status);
                }
            }
            else
            {
                childSection.phaseCode = 128;
                if (!StatusExtensions.IsUnenergized(childSection.status))
                {
                    StatusExtensions.Unenerize(childSection.status);
                    childSection.status += Constants.Unenergized;
                }
            }
        }

        public static bool DownsectionPCCompatibleWithSection(Section parentSection, Node childNode, int searchPhase, bool specifiedPhase)
        {
            int parentPC = parentSection.phaseCode;
            if (StatusExtensions.IsNetJunction(childNode.status))
            {
                return true;
            }
            if (specifiedPhase)
            {
                if (StatusExtensions.IsYDTransformer(childNode.phaseCode))
                {
                    if (PhaseCodeUtil.IsOnePhaseCodeDifferent(parentPC, childNode.phaseCode) && PhaseCodeUtil.IsPhaseCodePresent(childNode.phaseCode, searchPhase) && PhaseCodeUtil.IsPhaseCodePresent(parentPC, searchPhase))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (PhaseCodeUtil.IsPhaseCodePresent(parentSection.phaseCode, childNode.phaseCode) && PhaseCodeUtil.IsPhaseCodePresent(childNode.phaseCode, searchPhase))
                    return true;
                else
                    return false;
            }
            else
            {
                if (StatusExtensions.IsYDTransformer(childNode.phaseCode))
                {
                    if (PhaseCodeUtil.IsOnePhaseCodeDifferent(parentPC, childNode.phaseCode))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (PhaseCodeUtil.IsPhaseCodePresent(parentPC, childNode.phaseCode))
                    return true;
                else return false;
            }
        }

        public static bool MatchingParentSections(Node current, out List<Section> sections)
        {
            HashSet<Section> sectionList = new HashSet<Section>();
            sections = new List<Section>();
            bool matching = false;
            if (!string.IsNullOrEmpty(current.uid) && !StatusExtensions.IsUnenergized(current.status) && !StatusExtensions.IsDisabled(current.status) && current.parentList.Count > 1)
            {
                HashSet<int> pcList = new HashSet<int>();
                List<Section> parentList = current.parentList.Values.ToList();
                for (int i = 0; i < parentList.Count - 1; i++)
                {
                    for (int j = i + 1; j < parentList.Count; j++)
                    {
                        if ((parentList[i].phaseCode & parentList[j].phaseCode) != 128)
                        {
                            matching = true;
                            sectionList.Add(parentList[i]);
                            sectionList.Add(parentList[j]);
                        }
                    }
                }
            }
            sections = sectionList.ToList();
            return matching;
        }

        public static List<string> UpsectionGuidsToRoot(Node leaf, int commonPhase)
        {
            HashSet<Section> sectSet = new HashSet<Section>();
            List<string> path = new List<string>();
            if (leaf != null)
            {
                //Node leaf = _nodeList[guid];
                Stack<Node> s = new Stack<Node>();
                s.Push(leaf);
                while (s.Count > 0)
                {
                    Node current = s.Pop();
                    if (current == null)
                        continue;
                    if (!StatusExtensions.IsUnenergized(current.status) && !StatusExtensions.IsDisconnected(current.status) && !StatusExtensions.IsDisabled(current.status))
                    {
                        path.Add(current.uid);
                        foreach (var item in current.parentList.Values)
                        {
                            if (sectSet.Contains(item))
                                continue;
                            else if (!StatusExtensions.IsUnenergized(item.status) && !StatusExtensions.IsDisabled(item.status) && !StatusExtensions.IsDisconnected(item.status) && !StatusExtensions.IsLoop(item.status) && (commonPhase == -1 ? true : PhaseCodeUtil.IsPhaseCodePresent(item.phaseCode, commonPhase)))
                            {
                                sectSet.Add(item);
                                s.Push(item.parentNode);
                                path.Add(item.uid);
                                break;
                            }
                        }
                    }
                }
            }
            return path;
        }

        public static List<string> UpsectionGuidsToSub(Node leaf, int commonPhase)
        {
            HashSet<Section> sectSet = new HashSet<Section>();
            List<string> path = new List<string>();
            if (leaf != null)
            {
                //Node leaf = _nodeList[guid];
                Stack<Node> s = new Stack<Node>();
                s.Push(leaf);
                while (s.Count > 0)
                {
                    Node current = s.Pop();
                    if (current == null)
                        continue;
                    if (!StatusExtensions.IsUnenergized(current.status) && !StatusExtensions.IsDisconnected(current.status) && !StatusExtensions.IsDisabled(current.status))
                    {
                        path.Add(current.uid);
                        foreach (var item in current.parentList.Values)
                        {
                            if (sectSet.Contains(item))
                                continue;
                            else if (!StatusExtensions.IsUnenergized(item.status) && !StatusExtensions.IsDisabled(item.status) && !StatusExtensions.IsDisconnected(item.status) && !StatusExtensions.IsLoop(item.status))
                            {
                                sectSet.Add(item);
                                s.Push(item.parentNode);
                                path.Add(item.uid);
                                break;
                            }
                        }
                        if (!StatusExtensions.IsSource(current.status))
                        {
                            foreach (var item in current.childList.Values)
                            {
                                if (sectSet.Contains(item))
                                    continue;
                                else if (!StatusExtensions.IsUnenergized(item.status) && !StatusExtensions.IsDisabled(item.status) && !StatusExtensions.IsDisconnected(item.status) && !StatusExtensions.IsLoop(item.status))
                                {
                                    sectSet.Add(item);
                                    s.Push(item.childNode);
                                    path.Add(item.uid);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return path;
        }


        public static void LoopSet(HashSet<string> set)
        {            
            foreach (string guid in set)
            {
                if (StaticStuff._sectionList.ContainsKey(guid))
                {
                    Section sec = StaticStuff._sectionList[guid];
                    sec.status += Constants.Loop;
                    //if (StatusExtensions.IsEnergized(sec.status))
                    //    sec.status = Constants.Loop + Constants.Energized;
                    //if (StatusExtensions.IsUnenergized(sec.status))
                    //    sec.status = Constants.Loop + Constants.Unenergized;
                    //if (StatusExtensions.IsDisconnected(sec.status))
                    //    sec.status = Constants.Loop + Constants.Disconnected;
                    //if (StatusExtensions.IsDisabled(sec.status))
                    //    sec.status = Constants.Loop + Constants.Unenergized + Constants.Disabled;
                }
                else
                {
                    Node nd = StaticStuff._nodeList[guid];
                    nd.status += Constants.Loop;
                }
            }
        }

        public static bool ParentsEnergizedNoChild(Node nd)
        {
            bool incompatible = false;
            bool nochild = false;
            if (nd.childList == null || nd.childList.Count == 0) nochild = true;
            if (!(nd.parentList.Count == 0 || nd.parentList.Count == 1))
            {
                int count = 0;
                foreach (Section sec in nd.parentList.Values)
                {
                    if (StatusExtensions.IsEnergized(sec.status))
                    {
                        count++;
                    }
                }
                if (count > 1) incompatible = true;
            }
            return (incompatible && nochild);
        }

        public static bool HasChildren(Node nd)
        {
            if (nd.childList != null && nd.childList.Count > 0)
            {
                return true;
            }
            return false;
        }

        public static bool ChildUnenergized(Node nd)
        {
            foreach (Section sec in nd.childList.Values)
            {
                if (StatusExtensions.IsUnenergized(sec.status))
                    return true;
            }
            return false;
        }

        public static void PlaceOpenPoints(DataTable dtNetJnList)
        {
            foreach (Node nd in StaticStuff._nodeList.Values)
            {
                if (StatusExtensions.IsNetJunction(nd.status))
                {
                    int parentPC = GetParentPCSum(nd);
                    int childPC = GetChildPCSum(nd);
                    if (parentPC != childPC && PhaseCodeUtil.IsPhaseCodePresent(parentPC, childPC) && HasChildren(nd) && childPC != 128 && !ChildUnenergized(nd))
                    {
                        DataRow dr = dtNetJnList.Select("uid = '" + nd.uid + "'").FirstOrDefault();
                        //first update in memory network by replacing net junction with open point
                        nd.status = Constants.Energized;
                        nd.phaseCode = childPC;
                        nd.constructedPhaseCode = parentPC;
                        nd.oid = -2;
                        nd.classID = -2;
                        DataTable dt = dbutil.InsertOpenPt(nd);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            DataRow drOpenPt = dt.Rows[0];
                            nd.classID = (drOpenPt["classID"] == null || drOpenPt["classID"] == DBNull.Value) ? 0 : Convert.ToInt32(drOpenPt["classID"]);
                            nd.oid = (drOpenPt["oid"] == null || drOpenPt["oid"] == DBNull.Value) ? 0 : Convert.ToInt32(drOpenPt["oid"]);
                        }
                        //DataRow drOpenPts = dtOpenPts.NewRow();
                        //drOpenPts["x"] = (dr["x"] == null || dr["x"] == DBNull.Value) ? 0 : Convert.ToDouble(dr["x"]);
                        //drOpenPts["y"] = (dr["y"] == null || dr["y"] == DBNull.Value) ? 0 : Convert.ToDouble(dr["y"]);
                        //drOpenPts["Tox"] = DBNull.Value;
                        //drOpenPts["Toy"] = DBNull.Value;
                        //drOpenPts["oid"] = "-1";//update
                        //drOpenPts["uid"] = nd.uid;
                        //drOpenPts["adjuid"] = DBNull.Value;
                        //drOpenPts["phaseCode"] = nd.phaseCode;
                        //drOpenPts["constructedphaseCode"] = nd.constructedPhaseCode;
                        //drOpenPts["status"] = Constants.Energized;
                        //drOpenPts["type"] = 0;
                        //drOpenPts["ClassID"] = -2;//update
                        //dtOpenPts.Rows.Add(drOpenPts);
                        dr.Delete();
                    }
                }
            }
            dtNetJnList.AcceptChanges();
        }

        public static void DetectandDisableLoops()
        {
            List<INetworkObject> lst = new List<INetworkObject>();
            HashSet<string> disableSet = new HashSet<string>();
            List<INetworkObject> phaseIncompatibleSet = new List<INetworkObject>();
            foreach (Node item in StaticStuff._nodeList.Values)
            {
                if(item.uid == "{3470e302-c1e5-4cee-b1b7-522a38e4d0af}")
                {

                }
                List<Section> parentSections = new List<Section>();
                if (ParentsEnergizedNoChild(item))
                {
                    phaseIncompatibleSet.Add(item);
                    foreach (Section sec in item.parentList.Values)
                    {
                        sec.status = 128;
                        phaseIncompatibleSet.Add(sec);
                    }
                }
                else if (StatusExtensions.IsSource(item.status))
                {
                    foreach (Section sec in item.parentList.Values)
                    {
                        List<string> lstSource = UpsectionGuidsToSub(sec.parentNode, -1);
                        disableSet.Add(sec.uid);
                        disableSet.UnionWith(lstSource);
                    }
                    disableSet.Add(item.uid);
                }
                else if (item != null && !StatusExtensions.IsDisabled(item.status) && !StatusExtensions.IsDisconnected(item.status) && MatchingParentSections(item, out parentSections))
                {
                    string intersectionPt = "";
                    if (parentSections.Count > 1)
                    {
                        for (int i = 0; i < parentSections.Count - 1; i++)
                        {
                            for (int j = i + 1; j < parentSections.Count; j++)
                            {
                                int commonphase = PhaseCodeUtil.CommonPhase(parentSections[i].phaseCode, parentSections[j].phaseCode);
                                if (commonphase != 128)
                                {
                                    List<string> lst1 = UpsectionGuidsToRoot(parentSections[i].parentNode, commonphase);
                                    List<string> lst2 = UpsectionGuidsToRoot(parentSections[j].parentNode, commonphase);
                                    List<string> intersections = lst1.Intersect(lst2).ToList<string>();
                                    if (intersections.Count != 0)
                                    {
                                        intersectionPt = intersections[0];
                                        int i1 = lst1.IndexOf(intersectionPt);
                                        int i2 = lst2.IndexOf(intersectionPt);
                                        disableSet.Add(parentSections[i].uid);
                                        disableSet.Add(parentSections[j].uid);
                                        for (int k = 0; k < i1; k++) { disableSet.Add(lst1[k]); }
                                        for (int k = 0; k < i2; k++) { disableSet.Add(lst2[k]); }
                                    }
                                }
                            }
                        }
                    }

                }

            }
            LoopSet(disableSet);
            List<INetworkObject> statusUpdateList = new List<INetworkObject>();
            statusUpdateList.AddRange(_sectionList.Values.ToList<INetworkObject>());
            statusUpdateList.AddRange(_nodeList.Values.ToList<INetworkObject>());
            //dbutil.BulkUpdateStatus(statusUpdateList);            
        }


    }
}
