using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuturaNetwork
{
    public static class ClassIDRules
    {
        public static string capacitorBank = "capacitorBank";
        public static string Consumer = "Consumer";
        public static string DistributionSource = "DistributionSource";
        public static string Feeder = "Feeder";
        public static string FuseBank = "FuseBank";
        public static string OpenPoint = "OpenPoint";
        public static string PrimaryConductor = "PrimaryConductor";
        public static string recloserbank = "recloserbank";
        public static string regulatorbank = "regulatorbank";
        public static string SecondaryConductor = "SecondaryConductor";
        public static string SectionalizerBank = "SectionalizerBank";
        public static string StepTransformerBank = "StepTransformerBank";
        public static string SwitchBank = "SwitchBank";
        public static string TransformerBank = "TransformerBank";
        public static string NetJunction = "NetJunction";

        public static int[] adjArr = { 40, 42, 37 };//{ 128,130,137};//{147, 150, 152};
        public static int[] consArr = { 37 };//{ 130};//{147};
        public static int[] primary = { 46 };//{134 };//{156};
        public static int[] secondary = { 34 };//{ 135};//{144};
        public static HashSet<int> AdjGroup = new HashSet<int>(adjArr);
        public static HashSet<int> ConsumerGroup = new HashSet<int>(consArr); 

        public static bool AdjRuleSucceeds(Node nd, Section sec)
        {
            if (nd != null && nd.adjacentNode != null && AdjGroup.Contains(nd.adjacentNode.classID) && primary.Contains(sec.classID))
                return true;
            return false;
        }

        public static bool AdjRuleSucceeds(Section sec)
        {
            if (secondary.Contains(sec.classID))
                return true;
            return false;
        }

        public static bool IsAdjJunction(Node nd)
        {
            if (nd != null && nd.adjacentNode != null && AdjGroup.Contains(nd.adjacentNode.classID))
                return true;
            return false;
        }

        public static bool IsAdjGroupNode(Node nd)
        {
            if (nd != null &&  AdjGroup.Contains(nd.classID))
                return true;
            return false;
        }

        public static bool IsAdjGroupNode(int clsID)
        {
            if (AdjGroup.Contains(clsID))
                return true;
            return false;
        }

    }
}
