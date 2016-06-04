using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace FuturaNetwork
{
    public class DBUtil
    {

        public int sessionId = 1;
        public string conStr = @"initial catalog=moonwoody_small;data source=fta-nicholasg;user id=sa;password=n0tFuturaSa";
        public SqlParameter SPParameter(string dbfieldname, SqlDbType dbtype, int size, ParameterDirection paramDir, object value, SqlCommand sqlCmd)
        {
            SqlParameter p = new SqlParameter();
            p.ParameterName = dbfieldname;
            p.SqlDbType = dbtype;
            if (dbtype == SqlDbType.VarChar || dbtype == SqlDbType.NChar || dbtype == SqlDbType.NVarChar)
                p.Size = size;
            if (value != null)
                p.Value = value;
            else
            {
                p.IsNullable = true;
                p.Value = DBNull.Value;
                p.SqlValue = DBNull.Value;
            }
            p.Direction = paramDir;
            if (sqlCmd != null)
                sqlCmd.Parameters.Add(p);
            return p;
        }

        public DataTable GetDownStream()
        {
            DataSet dsResult = new DataSet();
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(conStr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "GetDownStream";
                    con.Open();
                    //cmd.CommandType = CommandType.StoredProcedure;
                    SPParameter("@Sessionid", SqlDbType.SmallInt, 500, ParameterDirection.Input, 1, cmd);
                    SPParameter("@uid", SqlDbType.VarChar, 500, ParameterDirection.Input, "{d0f21cd1-6850-4df1-b9f1-edb2f82b18a5}", cmd);
                    SPParameter("@ElementType", SqlDbType.SmallInt, 500, ParameterDirection.Input, 0, cmd);
                    SqlParameter op = SPParameter("@output", SqlDbType.VarChar, 500, ParameterDirection.Output, null, cmd);

                    SqlDataAdapter sqa = new SqlDataAdapter(cmd);
                    sqa.Fill(dsResult);
                    //var output = cmd.Parameters["@output"].Value;
                }
            }
            if (dsResult != null && dsResult.Tables != null && dsResult.Tables.Count == 1)
            {
                dt = dsResult.Tables[0];
            }
            return dt;
            //using (var da = new S())
            //{
            //    using (da.SelectCommand = conn.CreateCommand())
            //    {
            //        da.SelectCommand.CommandText = query;
            //        DataSet ds = new DataSet();
            //        da.Fill(ds);
            //        return ds.Tables[0];
            //    }
            //}
        }

        public DataTable GetConnectivity()
        {
            DataSet dsResult = new DataSet();
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(conStr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "fta_GetConnectivity";
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    SPParameter("@Sessionid", SqlDbType.SmallInt, 500, ParameterDirection.Input, 1, cmd);
                    SqlParameter op = SPParameter("@output", SqlDbType.VarChar, 500, ParameterDirection.Output, null, cmd);

                    SqlDataAdapter sqa = new SqlDataAdapter(cmd);
                    sqa.Fill(dsResult);
                    //var output = cmd.Parameters["@output"].Value;
                }
            }
            if (dsResult != null && dsResult.Tables != null && dsResult.Tables.Count == 1)
            {
                dt = dsResult.Tables[0];
            }
            return dt;
            //using (var da = new S())
            //{
            //    using (da.SelectCommand = conn.CreateCommand())
            //    {
            //        da.SelectCommand.CommandText = query;
            //        DataSet ds = new DataSet();
            //        da.Fill(ds);
            //        return ds.Tables[0];
            //    }
            //}
        }

        public DataTable FillClassIDs()
        {
            DataSet dsResult = new DataSet();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = conStr;
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select * from fta_classid";
            conn.Open();


            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(dsResult);
            if (dsResult != null && dsResult.Tables != null && dsResult.Tables.Count == 1)
            {
                dt = dsResult.Tables[0];
            }
            return dt;
        }

        public void PopulateClassID()
        {
            try
            {
                DataSet dsResult = new DataSet();
                var table = new DataTable();
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
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "fta_PopulateClassID";
                        con.Open();
                        //cmd.CommandType = CommandType.StoredProcedure;                   
                        SPParameter("@output", SqlDbType.VarChar, 500, ParameterDirection.Output, null, cmd);
                        SPParameter("@dbtype", SqlDbType.VarChar, 500, ParameterDirection.Input, "dbo", cmd);
                        SPParameter("@dbname", SqlDbType.VarChar, 500, ParameterDirection.Input, "moonwoody_small", cmd);
                        var features = new SqlParameter("@features", SqlDbType.Structured);
                        features.TypeName = "FeatureClassList";
                        features.Value = table;
                        cmd.Parameters.Add(features);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exc)
            {

            }
        }

        public void UpdateStatusAndPhase()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandTimeout = 10000;
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "fta_UdatePhaseStatus";
                        con.Open();
                        SPParameter("@output", SqlDbType.VarChar, 500, ParameterDirection.Output, null, cmd);
                        SPParameter("@guidColumn", SqlDbType.VarChar, 500, ParameterDirection.Input, "FuturaGUID", cmd);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exc)
            {

            }
        }

        public void PopulateElements()
        {
            try
            {
                DataSet dsResult = new DataSet();
                var table = new DataTable();
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
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandTimeout = 10000;
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "fta_PopulateElements";
                        con.Open();
                        //cmd.CommandType = CommandType.StoredProcedure;                   
                        SPParameter("@output", SqlDbType.VarChar, 500, ParameterDirection.Output, null, cmd);
                        SPParameter("@uid", SqlDbType.VarChar, 500, ParameterDirection.Input, "FuturaGUID", cmd);
                        SPParameter("@shapeColumn", SqlDbType.VarChar, 500, ParameterDirection.Input, "shape", cmd);
                        //SqlParameter features = SPParameter("@features", SqlDbType.Structured, 500, ParameterDirection.Input, null, cmd);
                        var features = new SqlParameter("@features", SqlDbType.Structured);
                        features.TypeName = "FeatureClassList";
                        features.Value = table;
                        cmd.Parameters.Add(features);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exc)
            {

            }
        }

        public DataTable GetConn()
        {
            DataSet dsResult = new DataSet();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = conStr;
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select * from fta_connectivity where sessionid is null or sessionid = " + sessionId + "order by sessionid asc";
            conn.Open();


            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(dsResult);
            return dsResult.Tables[0];

        }

        public DataTable GetSubstations(string tableName, string guidName)
        {
            DataSet dsResult = new DataSet();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = conStr;
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT objectid, CASE WHEN shape.STGeometryType() = 'LineString' THEN 1 WHEN shape.STGeometryType() = 'Point' THEN 0 END as type, enabled as status, phasecode, " + guidName + " as futuraguid, CASE WHEN shape.STGeometryType() = 'LineString' THEN shape.STStartPoint().STX WHEN shape.STGeometryType() = 'Point' THEN shape.STX END as X, CASE WHEN shape.STGeometryType() = 'LineString' THEN shape.STStartPoint().STY WHEN shape.STGeometryType() = 'Point' THEN shape.STY END as Y, CASE WHEN shape.STGeometryType() = 'LineString' THEN shape.STEndPoint().STX WHEN shape.STGeometryType() = 'Point' THEN NULL END as ToX, CASE WHEN shape.STGeometryType() = 'LineString' THEN shape.STEndPoint().STY WHEN shape.STGeometryType() = 'Point' THEN NULL END as ToY FROM " + tableName;
            conn.Open();

            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(dsResult);
            return dsResult.Tables[0];

        }

        public bool CreateElementsTable(int bucketCount)
        {
            try
            {
                string query = "CREATE TABLE [dbo].[FTA_Elements]([UID] [nvarchar](50) COLLATE Latin1_General_100_BIN2 NOT NULL,[Type] [smallint] NULL,[OID] [int] NULL,[X] [numeric](38, 8) NULL,[Y] [numeric](38, 8) NULL,[Tox] [numeric](38, 8) NULL,[Toy] [numeric](38, 8) NULL,[Status] [smallint] NULL,[PhaseCode] [smallint] NULL,[SessionID] [int] NULL,[Deleted] [smallint] NULL,[ShapeLength] [numeric](38, 8) NULL,[KW] [numeric](38, 8) NULL,[KWH] [numeric](38, 8) NULL,[KVA] [numeric](38, 8) NULL,[ClassName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,PRIMARY KEY NONCLUSTERED HASH ([UID])WITH ( BUCKET_COUNT = " + bucketCount + "))WITH ( MEMORY_OPTIMIZED = ON , DURABILITY = SCHEMA_AND_DATA )";
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.ConnectionString = conStr;
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                }
            }
            catch (Exception exc)
            {
                //log exc
            }
            return false;
        }

        public bool ApplySource()
        {
            try
            {
                DataSet dsResult = new DataSet();
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandTimeout = 10000;
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "fta_ApplySource";
                        con.Open();
                        SPParameter("@output", SqlDbType.VarChar, 500, ParameterDirection.Output, null, cmd);
                        SPParameter("@guidColumn", SqlDbType.VarChar, 500, ParameterDirection.Input, "FuturaGUID", cmd);
                        SPParameter("@source", SqlDbType.VarChar, 500, ParameterDirection.Input, "DistributionSource", cmd);
                        SPParameter("@whereClause", SqlDbType.VarChar, 500, ParameterDirection.Input, "ancillaryrole = 1", cmd);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception exc)
            {

            }
            return false;
        }

        public bool ApplyTransformer()
        {
            try
            {
                DataSet dsResult = new DataSet();
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandTimeout = 10000;
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "fta_ApplyTransformer";
                        con.Open();
                        SPParameter("@output", SqlDbType.VarChar, 500, ParameterDirection.Output, null, cmd);
                        SPParameter("@guidColumn", SqlDbType.VarChar, 500, ParameterDirection.Input, "FuturaGUID", cmd);
                        SPParameter("@transformer", SqlDbType.VarChar, 500, ParameterDirection.Input, "TransformerBank", cmd);
                        SPParameter("@whereClause", SqlDbType.VarChar, 500, ParameterDirection.Input, "windingcode = 5", cmd);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception exc)
            {

            }
            return false;
        }

        public bool BulkUpdateConnectivity(List<Section> lst)
        {
            DeleteConnectivityAll();
            //split list into 3 lists
            List<Section> list1 = new List<Section>();
            List<Section> list2 = new List<Section>();
            List<Section> list3 = new List<Section>();
            list1 = lst.GetRange(0, lst.Count / 3);
            list2 = lst.GetRange(lst.Count / 3, ((2 * lst.Count / 3) - (lst.Count / 3)));
            list3 = lst.GetRange(2 * lst.Count / 3, lst.Count - (2 * lst.Count / 3));
            bool result = BulkUpdateConnectivityOnLists(list1);
            if (!result) return false;
            result = BulkUpdateConnectivityOnLists(list2);
            if (!result) return false;
            result = BulkUpdateConnectivityOnLists(list3);
            return result;
        }

        public bool BulkUpdateConnectivityOnLists(List<Section> lst)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Clear();
                dt.Columns.Add("id");
                dt.Columns.Add("uid");
                dt.Columns.Add("PhaseCode");
                dt.Columns.Add("status");
                dt.Columns.Add("ParentNode");
                dt.Columns.Add("PNstatus");
                dt.Columns.Add("PNPhaseCode");
                dt.Columns.Add("ChildNode");
                dt.Columns.Add("CNStatus");
                dt.Columns.Add("CNPhaseCode");
                dt.Columns.Add("SessionID");
                dt.Columns.Add("deleted");
                foreach (Section sec in lst)
                {
                    if (sec.uid != null)
                    {
                        DataRow dr = dt.NewRow();
                        dr["uid"] = sec.uid;
                        dr["phaseCode"] = sec.phaseCode;
                        dr["status"] = sec.status;
                        dr["ParentNode"] = sec.parentNode != null ? sec.parentNode.uid : "NULL";
                        dr["PNstatus"] = sec.parentNode != null ? sec.parentNode.status : 0;
                        dr["PNPhaseCode"] = sec.parentNode != null ? sec.parentNode.phaseCode : 0;
                        dr["ChildNode"] = sec.childNode != null ? sec.childNode.uid : "NULL";
                        dr["CNStatus"] = sec.childNode != null ? sec.childNode.status : 0;
                        dr["CNPhaseCode"] = sec.childNode != null ? sec.childNode.phaseCode : 0;
                        dr["SessionID"] = Constants.SessionID;
                        dr["deleted"] = 0;
                        dt.Rows.Add(dr);
                    }
                    else
                    {
                    }
                }
                dt.AcceptChanges();

                using (SqlConnection connection =
        new SqlConnection(conStr))
                {
                    SqlBulkCopy bulkCopy =
                        new SqlBulkCopy
                        (
                        connection,
                        SqlBulkCopyOptions.UseInternalTransaction,
                        null
                        );
                    bulkCopy.BulkCopyTimeout = 10000;
                    bulkCopy.DestinationTableName = "fta_connectivity";
                    connection.Open();
                    bulkCopy.WriteToServer(dt);
                    connection.Close();
                }
                // reset
                dt.Clear();
                return true;
            }
            catch (Exception exc)
            {
                //log exc
            }
            return false;
        }

        public bool InsertConnectivity(Section sec)
        {
            try
            {
                string query = "insert into fta_connectivity  WITH (SNAPSHOT) (uid, PhaseCode, ParentNode, PNstatus, PNPhaseCode, ChildNode, CNStatus, CNPhaseCode,status,sessionid, deleted) VALUES(" + sec.uid + "," + sec.phaseCode + "," + sec.parentNode != null ? sec.parentNode.uid : DBNull.Value +
                   "," + sec.parentNode != null ? sec.parentNode.status.ToString() : "0" + "," + sec.parentNode != null ? sec.parentNode.phaseCode.ToString() : "0" +
                   "," + sec.childNode != null ? sec.childNode.uid : DBNull.Value + "," + sec.childNode != null ? sec.childNode.status.ToString() : "0" + "," + sec.childNode != null ? sec.childNode.phaseCode.ToString() : "0" +
                        ", " + sec.status + "," + Constants.SessionID + "," + 0 + ")";
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.ConnectionString = conStr;
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                }
            }
            catch (Exception exc)
            {
                //log exc
            }
            return false;
        }

        public bool BulkDeleteConnectivity(List<string> lst)
        {
            try
            {
                string query = "update fta_connectivity  WITH (SNAPSHOT) set deleted = 1 where uid in (";
                foreach (string guid in lst)
                {
                    query = query + guid + ", ";
                }
                query = query + ")";
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.ConnectionString = conStr;
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                }
            }
            catch (Exception exc)
            {
                //log exc
            }
            return false;
        }

        public bool DeleteConnectivity(Section sec)
        {
            try
            {
                string query = "update fta_connectivity  WITH (SNAPSHOT) set deleted = 1 where uid = " + sec.uid;
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.ConnectionString = conStr;
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                }
            }
            catch (Exception exc)
            {
                //log exc
            }
            return false;
        }

        public bool DeleteConnectivityAll()
        {
            try
            {
                string query = "delete from fta_connectivity  WITH (SNAPSHOT)";
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.ConnectionString = conStr;
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                }
            }
            catch (Exception exc)
            {
                //log exc
            }
            return false;
        }

        //public bool BulkUpdateConnectivity(List<Section> lst)
        //{
        //    try
        //    {
        //        foreach (Section sec in lst)
        //        {
        //            string query = "update fta_connectivity  WITH (SNAPSHOT) set PhaseCode = " + sec.phaseCode + ", ParentNode = " + sec.ParentNode != null ? sec.ParentNode.uid : DBNull.Value +
        //               ", PNstatus = " + sec.ParentNode != null ? sec.ParentNode.status.ToString() : "0" + ", PNPhaseCode =" + sec.ParentNode != null ? sec.ParentNode.phaseCode.ToString() : "0" +
        //               ", ChildNode = " + sec.ChildNode != null ? sec.ChildNode.uid : DBNull.Value + ", CNStatus = " + sec.ChildNode != null ? sec.ChildNode.status.ToString() : "0" + ", CNPhaseCode =" + sec.ChildNode != null ? sec.ChildNode.phaseCode.ToString() : "0" +
        //                    ",status = " + sec.status + ",sessionid = " + Constants.SessionID + ", deleted = " + 0 +
        //                    " where uid = " + sec.uid;
        //            using (SqlConnection conn = new SqlConnection(conStr))
        //            {
        //                using (SqlCommand cmd = new SqlCommand(query, conn))
        //                {
        //                    conn.ConnectionString = conStr;
        //                    cmd.Connection = conn;
        //                    cmd.CommandType = CommandType.Text;
        //                    conn.Open();
        //                    cmd.ExecuteNonQuery();
        //                    conn.Close();
        //                    return true;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception exc)
        //    {
        //        //log exc
        //    }
        //    return false;
        //}

        public bool UpdateConnectivity(Section sec)
        {
            try
            {
                string query = "update fta_connectivity  WITH (SNAPSHOT) set PhaseCode = " + sec.phaseCode + ", ParentNode = " + sec.parentNode != null ? sec.parentNode.uid : DBNull.Value +
                   ", PNstatus = " + sec.parentNode != null ? sec.parentNode.status.ToString() : "0" + ", PNPhaseCode =" + sec.parentNode != null ? sec.parentNode.phaseCode.ToString() : "0" +
                   ", ChildNode = " + sec.childNode != null ? sec.childNode.uid : DBNull.Value + ", CNStatus = " + sec.childNode != null ? sec.childNode.status.ToString() : "0" + ", CNPhaseCode =" + sec.childNode != null ? sec.childNode.phaseCode.ToString() : "0" +
                        ",status = " + sec.status + ",sessionid = " + Constants.SessionID + ", deleted = " + 0 +
                        " where uid = " + sec.uid;
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.ConnectionString = conStr;
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                }
            }
            catch (Exception exc)
            {
                //log exc
            }
            return false;
        }

        public bool InsertElementSection(Section sec)
        {
            try
            {
                string query = "insert into fta_elements WITH (SNAPSHOT)(uid, oid, x, y,status,sessionid, deleted) VALUES(" + sec.uid + "," + sec.oid + "," + sec.x + "," + sec.y +
                        "," + sec.status + "," + Constants.SessionID + "," + 0 + ")";
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.ConnectionString = conStr;
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                }
            }
            catch (Exception exc)
            {
                //log exc
            }
            return false;
        }

        public bool DeleteElementSection(Section sec)
        {
            try
            {
                string query = "update fta_elements WITH (SNAPSHOT) set deleted = 1 where uid =" + sec.uid;
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.ConnectionString = conStr;
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                }
            }
            catch (Exception exc)
            {
                //log exc
            }
            return false;
        }

        public bool BulkInsertElements(List<INetworkObject> updateList)
        {
            try
            {
                DataTable dtNetJnList = new DataTable();
                dtNetJnList.Clear();
                dtNetJnList.Columns.Add("uid");
                dtNetJnList.Columns.Add("type");
                dtNetJnList.Columns.Add("oid");
                dtNetJnList.Columns.Add("x");
                dtNetJnList.Columns.Add("y");
                dtNetJnList.Columns.Add("tox");
                dtNetJnList.Columns.Add("toy");
                dtNetJnList.Columns.Add("status");
                dtNetJnList.Columns.Add("PhaseCode");
                dtNetJnList.Columns.Add("SessionID");
                dtNetJnList.Columns.Add("deleted");
                //dtNetJnList.Columns.Add("ClassName");
                foreach (var item in updateList)
                {
                    DataRow drNetJn = dtNetJnList.NewRow();
                    if (item.GetType() == typeof(Node))
                    {
                        Node nd = (Node)item;
                        drNetJn["x"] = nd.x;
                        drNetJn["y"] = nd.y;
                        drNetJn["Tox"] = DBNull.Value;
                        drNetJn["Toy"] = DBNull.Value;
                        drNetJn["uid"] = nd.uid;
                        drNetJn["phaseCode"] = nd.phaseCode;
                        drNetJn["status"] = nd.status;
                        drNetJn["type"] = 0;
                        dtNetJnList.Rows.Add(drNetJn);
                    }
                    if (item.GetType() == typeof(Section))
                    {
                        Section sec = (Section)item;
                        drNetJn["x"] = sec.x;
                        drNetJn["y"] = sec.y;
                        drNetJn["Tox"] = sec.tox;
                        drNetJn["Toy"] = sec.toy;
                        drNetJn["uid"] = sec.uid;
                        drNetJn["phaseCode"] = sec.phaseCode;
                        drNetJn["status"] = sec.status;
                        drNetJn["type"] = 1;
                        dtNetJnList.Rows.Add(drNetJn);
                    }
                }
                dtNetJnList.AcceptChanges();
                using (SqlConnection connection =
        new SqlConnection(conStr))
                {
                    SqlBulkCopy bulkCopy =
                        new SqlBulkCopy
                        (
                        connection,
                        SqlBulkCopyOptions.UseInternalTransaction,
                        null
                        );

                    bulkCopy.DestinationTableName = "fta_elements";
                    connection.Open();
                    bulkCopy.WriteToServer(dtNetJnList);
                    connection.Close();
                }
                dtNetJnList.Clear();
                return true;
            }
            catch (Exception exc)
            {
                //log exc
            }
            return false;
        }

        public bool BulkUpdateStatus(List<INetworkObject> updateList)
        {
            try
            {
                Dictionary<int, List<INetworkObject>> statusDict = new Dictionary<int, List<INetworkObject>>();
                foreach (INetworkObject guid in updateList)
                {
                    if (statusDict.ContainsKey(guid.status))
                        statusDict[guid.status].Add(guid);
                    else
                    {
                        statusDict.Add(guid.status, new List<INetworkObject>());
                        statusDict[guid.status].Add(guid);
                    }
                }
                foreach (int status in statusDict.Keys)
                {
                    StringBuilder queryBuilder = new StringBuilder();
                    queryBuilder.Append("update fta_elements  WITH (SNAPSHOT) set status = " + status + " where uid in (");
                    foreach (INetworkObject guid in statusDict[status])
                    {
                        if (guid != null)
                            queryBuilder.Append("'" + guid.uid + "' ,");
                    }
                    string query = queryBuilder.ToString();
                    query = query.TrimEnd(',');
                    query = query + ")";
                    using (SqlConnection conn = new SqlConnection(conStr))
                    {
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandTimeout = 10000;
                            conn.ConnectionString = conStr;
                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.Text;
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                }
                return true;
            }
            catch (Exception exc)
            {
                //log exc
            }
            return false;
        }

        public bool BulkInsertElementNodes(DataTable dt)
        {
            try
            {
                using (SqlConnection connection =
        new SqlConnection(conStr))
                {
                    SqlBulkCopy bulkCopy =
                        new SqlBulkCopy
                        (
                        connection,
                        SqlBulkCopyOptions.UseInternalTransaction,
                        null
                        );

                    bulkCopy.DestinationTableName = "fta_elements";
                    connection.Open();
                    bulkCopy.WriteToServer(dt);
                    connection.Close();
                }
                dt.Clear();
                return true;
            }
            catch (Exception exc)
            {
                //log exc
            }
            return false;
        }

        public bool DeleteElementNode(Node nd)
        {
            try
            {
                string query = "update fta_elements set deleted = 1 where uid =" + nd.uid;
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.ConnectionString = conStr;
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                }
            }
            catch (Exception exc)
            {
                //log exc
            }
            return false;
        }

        public DataTable InsertOpenPt(Node nd)
        {
            DataSet dsResult = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandTimeout = 10000;
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "fta_InsertOpenPoint";
                        con.Open();
                        SPParameter("@output", SqlDbType.VarChar, 500, ParameterDirection.Output, null, cmd);
                        SPParameter("@guidColumn", SqlDbType.VarChar, 500, ParameterDirection.Input, "FuturaGUID", cmd);
                        SPParameter("@phasecode", SqlDbType.Int, 0, ParameterDirection.Input, nd.phaseCode, cmd);
                        SPParameter("@constructedphasecode", SqlDbType.Int, 0, ParameterDirection.Input, nd.constructedPhaseCode, cmd);
                        SPParameter("@uid", SqlDbType.VarChar, 500, ParameterDirection.Input, nd.uid, cmd);
                        SPParameter("@classname", SqlDbType.VarChar, 500, ParameterDirection.Input, "openpoint", cmd);
                        SPParameter("@sessionid", SqlDbType.Int, 0, ParameterDirection.Input, Constants.SessionID, cmd);
                        SPParameter("@status", SqlDbType.Int, 0, ParameterDirection.Input, nd.status, cmd);
                        SPParameter("@enabled", SqlDbType.Int, 0, ParameterDirection.Input, StatusExtensions.IsDisabled(nd.status) ? 0 : 1, cmd);
                        SPParameter("@deleted", SqlDbType.Int, 0, ParameterDirection.Input, 0, cmd);
                        SPParameter("@x", SqlDbType.Float, 0, ParameterDirection.Input, nd.x, cmd);
                        SPParameter("@y", SqlDbType.Float, 0, ParameterDirection.Input, nd.y, cmd);
                        SqlDataAdapter sda = new SqlDataAdapter(cmd);
                        sda.Fill(dsResult);
                    }
                }
                if (dsResult != null && dsResult.Tables != null && dsResult.Tables.Count == 1)
                {
                    dt = dsResult.Tables[0];
                }

            }
            catch (Exception exc)
            {
                //log exc
            }
            return dt;
        }

        public DataTable GetElements()
        {
            DataSet dsResult = new DataSet();
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(conStr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandTimeout = 1000;
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "fta_GetElements";
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    SPParameter("@Sessionid", SqlDbType.SmallInt, 500, ParameterDirection.Input, 1, cmd);
                    SqlParameter op = SPParameter("@output", SqlDbType.VarChar, 500, ParameterDirection.Output, null, cmd);

                    SqlDataAdapter sqa = new SqlDataAdapter(cmd);
                    sqa.Fill(dsResult);
                    //var output = cmd.Parameters["@output"].Value;
                }
            }
            if (dsResult != null && dsResult.Tables != null && dsResult.Tables.Count == 1)
            {
                dt = dsResult.Tables[0];
            }
            return dt;

        }

        public DataTable dtAsync = new DataTable();

        public int GetCount()
        {
            DataSet dsResult = new DataSet();
            bool flag = false;
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = conStr;

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select count(*) from fta_connectivity";
            conn.Open();
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(dsResult);
            DateTime dtend = DateTime.Now;
            return int.Parse(dsResult.Tables[0].Rows[0][0].ToString());

        }
    }
}
