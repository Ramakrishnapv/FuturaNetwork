using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FuturaNetwork
{
    public class NTInfo
    {
        public List<string> networkClasses;
        public string shapeColumn;
        public DataTable featureList = new DataTable();
        public string networkName;
        public Dictionary<string, int> networkClassIds;
        public HashSet<string> AdjGroup;
        public string statusField;
        public double snappingTolerance;
        public string phaseCodeField;
        public string sourceWhereClause;
        public string networkProtectorWhereClause;
        public List<Weight> weights;
        public string guidName;
        public string conStr;
        public string connectivityTable;
        public string elementsTable;
        public string constructedPhaseField;
        public Dictionary<int, int> guidFldIndexByClsId;
        public Dictionary<int, int> phFldIndexByClsId;
        public Dictionary<int, int> constPhFldIndexByClsId;
        public Source Source;
        public NetworkProtector NTProtector;
        public YDTransformer YDTransformer;
        public int LightCLSID;
        public int StructureCLSID;
        public string WorkspaceID;
    }
}
