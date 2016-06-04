using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Futura.ArcGIS.NetworkExtension
{
    public class Tracing : ESRI.ArcGIS.Desktop.AddIns.ComboBox
    {
        public Tracing()
        {
            this.Add("Trace Down");
            this.Add("Trace Up");
        }

        protected override void OnUpdate()
        {
            Enabled = (ExtensionInfo.Extension != null && ExtensionInfo.Extension.ExtensionState == ESRI.ArcGIS.Desktop.AddIns.ExtensionState.Enabled);
        }

        
    }

}
