using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using UndoRedoFramework.Collections.Generic;

namespace FuturaNetwork
{
    public static class StaticStuff
    {
        public static int sessionId = 1;
        //public delegate DataTable AsyncMethodCaller();
        public static Dictionary<string, Node> _nodeList = new UndoRedoDictionary<string, Node>();
        public static Dictionary<string, Section> _sectionList = new UndoRedoDictionary<string, Section>();
        public static DBUtil dbutil = new DBUtil();
        public static NTInfo ntInfo = new NTInfo();
 
        public static void InitializeNTInfo()
        {
            DataTable table = new DataTable();
            table.Columns.Add("item", typeof(string));
            table.Rows.Add("capacitorBank");
            table.Rows.Add("Consumer");
            table.Rows.Add("DistributionSource");
            table.Rows.Add("Feeder");
            table.Rows.Add("FuseBank");
            table.Rows.Add("OpenPoint");
            table.Rows.Add("PrimaryConductor");
            table.Rows.Add("recloserbank");
            table.Rows.Add("regulatorbank");
            table.Rows.Add("SecondaryConductor");
            table.Rows.Add("SectionalizerBank");
            table.Rows.Add("StepTransformerBank");
            table.Rows.Add("SwitchBank");
            table.Rows.Add("TransformerBank");
            ntInfo.featureList = table;
            ntInfo.guidName = "FuturaGUID";
            ntInfo.phaseCodeField = "PhaseCode";
            ntInfo.constructedPhaseField = "constructedPhase";
            ntInfo.shapeColumn = "Shape";
            ntInfo.snappingTolerance = 0.003;
            ntInfo.conStr = @"initial catalog=moonwoody_small;data source=fta-nicholasg;user id=sa;password=n0tFuturaSa";
            ntInfo.connectivityTable = "fta_connectivity";
            ntInfo.elementsTable = "fta_elements";
            ntInfo.AdjGroup = new HashSet<string>();
            ntInfo.AdjGroup.Add("capacitorBank");
            ntInfo.AdjGroup.Add("StepTransformerBank");
            ntInfo.AdjGroup.Add("TransformerBank");
            FillClassIDs();
        }


        public static void FillClassIDs()
        {
            dbutil.PopulateClassID();
            ntInfo.networkClassIds = new Dictionary<string, int>();
            DataTable dt = dbutil.FillClassIDs();
            foreach (DataRow dr in dt.Rows)
            {
                ntInfo.networkClassIds.Add(dr["ClassName"].ToString(), Convert.ToInt32(dr["ClassID"]));
            }
        }

        public static void FillElementsFromDB()
        {
            DataTable dtElements = dbutil.GetElements();
            foreach (DataRow dr in dtElements.Rows)
            {
                int type = (dr["type"] == null || dr["type"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["type"]);
                string guid = (dr["UID"] == null || dr["UID"] == DBNull.Value) ? "" : dr["UID"].ToString();
                string adjuid = (dr["adjuid"] == null || dr["adjuid"] == DBNull.Value) ? "" : dr["adjuid"].ToString();
                int oid = (dr["oid"] == null || dr["oid"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["oid"]);
                int status = ((dr["status"] == null || dr["status"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["status"]));
                int phaseCode = (dr["phaseCode"] == null || dr["phaseCode"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["phaseCode"]);
                int constructedPhaseCode = (dr["constructedPhaseCode"] == null || dr["constructedPhaseCode"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["constructedPhaseCode"]);
                double x = (dr["x"] == null || dr["x"] == DBNull.Value) ? 0 : Convert.ToDouble(dr["x"]);
                double y = (dr["y"] == null || dr["y"] == DBNull.Value) ? 0 : Convert.ToDouble(dr["y"]);
                double toX = (dr["toX"] == null || dr["toX"] == DBNull.Value) ? 0 : Convert.ToDouble(dr["toX"]);
                double toY = (dr["toY"] == null || dr["toY"] == DBNull.Value) ? 0 : Convert.ToDouble(dr["toY"]);
                int classID = (dr["classID"] == null || dr["classID"] == DBNull.Value) ? 0 : Convert.ToInt32(dr["classID"]);
                if (guid == "{9f9bb3e5-95ff-42c3-88af-a5e109004196}")
                {

                }
                //fill nodes and sections
                if (type == 0 ) //fill node list
                {
                    if (!_nodeList.ContainsKey(guid))
                    {
                        Node nd = new Node();
                        nd.uid = guid;
                        if (!string.IsNullOrEmpty(adjuid))
                        {
                            if (!_nodeList.ContainsKey(adjuid))
                            {
                                Node adjnd = new Node();
                                adjnd.uid = adjuid;
                                adjnd.isAdj = true;
                                nd.adjacentNode = adjnd;
                                _nodeList.Add(adjuid, adjnd);
                            }
                            else
                            {
                                nd.adjacentNode = _nodeList[adjuid];
                            }

                        }
                        nd.x = x;
                        nd.y = y;
                        nd.objectId = oid;
                        nd.oid = oid;
                        nd.status = status;
                        nd.phaseCode = phaseCode;
                        nd.constructedPhaseCode = constructedPhaseCode;
                        nd.classID = classID;
                        _nodeList.Add(guid, nd);
                    }
                    else
                    {
                        Node nd = _nodeList[guid];
                        nd.x = x;
                        nd.y = y;
                        nd.oid = oid;
                        nd.objectId = oid;
                        nd.status = status;
                        nd.phaseCode = phaseCode;
                        nd.constructedPhaseCode = constructedPhaseCode;
                        nd.classID = classID;
                        if (!string.IsNullOrEmpty(adjuid))
                        {
                            if (!_nodeList.ContainsKey(adjuid))
                            {
                                Node adjnd = new Node();
                                adjnd.uid = adjuid;
                                adjnd.isAdj = true;
                                nd.adjacentNode = adjnd;
                            }
                            else
                            {
                                nd.adjacentNode = _nodeList[adjuid];
                            }

                        }
                    }
                }

                else if (type == 1 && !_sectionList.ContainsKey(guid))
                {
                    Section sec = new Section();
                    sec.uid = guid;
                    sec.x = x;
                    sec.y = y;
                    sec.tox = toX;
                    sec.toy = toY;
                    sec.objectId = oid;
                    sec.oid = oid;
                    sec.status = status;
                    sec.classID = classID;
                    sec.phaseCode = phaseCode;
                    sec.constructedPhaseCode = constructedPhaseCode;
                    _sectionList.Add(guid, sec);
                }
            }
        }


    }
}
