using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using FuturaNetwork;
using ESRI.ArcGIS.Editor;

namespace Futura.ArcGIS.NetworkExtension
{
    public class FuturaNetWorkExt : ESRI.ArcGIS.Desktop.AddIns.Extension
    {
        public const string sqlConnStr = "";

        public CreateNetwork crNet = new CreateNetwork();
        public NetworkUtil netUtil = ExtensionInfo.netUtil;

        private WorkspaceListener wsl;
        public FuturaNetWorkExt()
        {
        }

        protected override void OnStartup()
        {
            ExtensionInfo.Extension = this;
            ExtensionInfo.Editor = GetEditor();
            wsl = new WorkspaceListener(ExtensionInfo.Editor);
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
            //crNet.Create();
           

 
            netUtil.LoadData();
            ExtensionInfo.checkFlow = new CheckFlow();
            //NetworkUtil.DetectandDisableLoops();
            //List<INetworkObject> lst = netUtil.DownStream("{0495de99-3bc8-4122-b51a-7089f5ea7810}", 0, -1, "");
            netUtil.PopulateConductors();

        }

        void FuturaNetWorkExt_OnStartEditing()
        {
            
        }

        void ArcMap_NewDocument()
        {
            // TODO: Handle new document event
        }

        public ESRI.ArcGIS.Desktop.AddIns.ExtensionState ExtensionState
        {
            get { return base.State; }
        }


        private IEditor3 GetEditor()
        {
            UID editorUid = new UID();
            editorUid.Value = "esriEditor.Editor";
            return ArcMap.Application.FindExtensionByCLSID(editorUid) as IEditor3;
        }
    }

}
