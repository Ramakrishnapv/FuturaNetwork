using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Carto;


namespace Futura.ArcGIS.NetworkExtension
{
    public class FindDisconnected : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public FindDisconnected()
        {
        }

        protected override void OnClick()
        {
            ArcMap.Document.FocusMap.ClearSelection();
            List<FuturaNetwork.INetworkObject> lst = ExtensionInfo.netUtil.FindDisconnected();
            IDictionary<int, IList<int>> traceFeaturesByClassId = new Dictionary<int, IList<int>>();
            foreach (FuturaNetwork.INetworkObject to in lst)
            {
                if (traceFeaturesByClassId.ContainsKey(to.classID))
                {
                    if (traceFeaturesByClassId[to.classID] != null && !traceFeaturesByClassId[to.classID].Contains(to.oid))
                        traceFeaturesByClassId[to.classID].Add(to.oid);
                }
                else
                {
                    IList<int> oids = new List<int>();
                    oids.Add(to.oid);
                    traceFeaturesByClassId.Add(to.classID, oids);
                }
            }

            IList<ILayer> layers = DisplayMap.GetMapLayersByLayerType(ArcMap.Document.FocusMap, LayerType.FeatureLayer, true);
            if (layers != null && layers.Count > 0)
            {
                for (int lyr = 0; lyr < layers.Count; lyr++)
                {
                    if (layers[lyr] != null && layers[lyr].Valid && layers[lyr] is IFeatureLayer)
                    {
                        IFeatureLayer featLyr = layers[lyr] as IFeatureLayer;
                        if (featLyr != null && featLyr.FeatureClass != null //&& string.Compare(featLyr.FeatureClass.AliasName, FuturaNetJunctions.Futura_NetJunctions_TableName, true) != 0
                            && traceFeaturesByClassId.ContainsKey(featLyr.FeatureClass.FeatureClassID)
                            && traceFeaturesByClassId[featLyr.FeatureClass.FeatureClassID] != null && traceFeaturesByClassId[featLyr.FeatureClass.FeatureClassID].Count > 0)
                        {
                            //get the array of OIDs for this layer
                            int[] oids = new int[traceFeaturesByClassId[featLyr.FeatureClass.FeatureClassID].Count];
                            for (int ids = 0; ids < traceFeaturesByClassId[featLyr.FeatureClass.FeatureClassID].Count; ids++)
                            {
                                oids[ids] = traceFeaturesByClassId[featLyr.FeatureClass.FeatureClassID][ids];
                            }
                            DisplayMap.AddFeaturesToSelection(featLyr, oids);
                        }
                    }
                }
            }

            ArcMap.Document.ActiveView.Refresh();
        }

        protected override void OnUpdate()
        {
            Enabled = (ExtensionInfo.Extension != null && ExtensionInfo.Extension.ExtensionState == ESRI.ArcGIS.Desktop.AddIns.ExtensionState.Enabled);
        }
    }
}
