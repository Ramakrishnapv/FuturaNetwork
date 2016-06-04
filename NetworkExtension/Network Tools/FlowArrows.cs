using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace Futura.ArcGIS.NetworkExtension
{
    public class FlowArrows : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        private bool enableFlowDir = false;
        DisplayFlowDirections flowDir = null;
        public FlowArrows()
        {
        }

        protected override void OnClick()
        {
            if (!enableFlowDir)
            {
                if (flowDir == null)
                {
                    flowDir = new DisplayFlowDirections(ArcMap.Document.FocusMap, ArcMap.Document.ActiveView);
                }
                flowDir.DisplayPrimaryFlowDirections();
                flowDir.DisplaySecondaryFlowDirections();
                enableFlowDir = true;
            }
            else
            {
                flowDir.ClearPrimaryFlowDirections();
                flowDir.ClearSecondaryFlowDirections();
                enableFlowDir = false;
            }
        }

        protected override void OnUpdate()
        {
            Enabled = (ExtensionInfo.Extension != null && ExtensionInfo.Extension.ExtensionState == ESRI.ArcGIS.Desktop.AddIns.ExtensionState.Enabled);
        }
    }
}
