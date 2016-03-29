using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NetworkExtension
{
    public class FuturaNetWorkExt : ESRI.ArcGIS.Desktop.AddIns.Extension
    {
        public const string sqlConnStr = "";
        public FuturaNetWorkExt()
        {
        }

        protected override void OnStartup()
        {
            //
            // TODO: Uncomment to start listening to document events
            //
             WireDocumentEvents();
        }

        private void WireDocumentEvents()
        {
            //
            // TODO: Sample document event wiring code. Change as needed
            //
            ArcMap.Events.OpenDocument += delegate() { ArcMap_OpenDocument(); };

            // Named event handler
            ArcMap.Events.NewDocument += delegate() { ArcMap_NewDocument(); };

            //// Anonymous event handler
            //ArcMap.Events.BeforeCloseDocument += delegate()
            //{
            //    // Return true to stop document from closing
            //    ESRI.ArcGIS.Framework.IMessageDialog msgBox = new ESRI.ArcGIS.Framework.MessageDialogClass();
            //    return msgBox.DoModal("BeforeCloseDocument Event", "Abort closing?", "Yes", "No", ArcMap.Application.hWnd);
            //};

        }

        void ArcMap_OpenDocument()
        {
            //if (base.State == ESRI.ArcGIS.Desktop.AddIns.ExtensionState.Enabled)
            //{

            //}

        }

        void ArcMap_NewDocument()
        {
            // TODO: Handle new document event
        }

    }

}
