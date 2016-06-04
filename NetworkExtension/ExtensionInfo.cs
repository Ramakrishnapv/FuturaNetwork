using System;
using System.Drawing;
using System.Resources;
using System.Reflection;
using System.Collections.Generic;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using FuturaNetwork;


namespace Futura.ArcGIS.NetworkExtension
{

    #region Delegates
    public delegate void SketchStatusUpdateEventHandler(double locationX, double locationY, double arithAngle, double geoAngle, double lineLength, double segmentLength);
    #endregion

    public static class ExtensionInfo
    {
        #region Variables
        private static List<EditObject> _editQueue = new List<EditObject>();
        public static IDictionary<int, string> ClassNamesBySubType = new Dictionary<int, string>();
        private static Dictionary<string, IFeatureLayer> _featLyrByFCName = new Dictionary<string, IFeatureLayer>();
        private static Dictionary<string, IDatasetName> _dsNames = new Dictionary<string, IDatasetName>();
        public static NetworkUtil _netUtil = new NetworkUtil();
        public static CheckFlow _checkFlow;
        public static bool InternalEdit = false;
        public static bool UpdateConnectivity = true;
        #endregion

        #region Internal Properties
        internal static List<EditObject> EditQueue
        {
            get { return _editQueue; }
            set { _editQueue = value; }
        }

        internal static NetworkUtil netUtil
        {
            get { return _netUtil; }
            set { _netUtil = value; }
        }

        internal static CheckFlow checkFlow
        {
            get { return _checkFlow; }
            set { _checkFlow = value; }
        }

        internal static Dictionary<string, IFeatureLayer> FeatureLyrByFCName  // fill this on start editing
        {
            get { return _featLyrByFCName; }
            set { _featLyrByFCName = value; }
        }

        internal static Dictionary<string, IDatasetName> DatasetNames  // fill this on start editing
        {
            get { return _dsNames; }
            set { _dsNames = value; }
        }

        internal static FuturaNetWorkExt Extension { get; set; }

        internal static ESRI.ArcGIS.Editor.IEditor3 Editor { get; set; }

        internal static IActiveView ActiveView
        {
            get
            {
                return ArcMap.Document.ActiveView;
            }
        }


        internal static bool IsExtensionEnabled
        {
            get
            {
                bool isEnabled = false;
                if (Extension != null)
                {
                    if (Extension.ExtensionState == ESRI.ArcGIS.Desktop.AddIns.ExtensionState.Enabled)
                        isEnabled = true;
                }
                return isEnabled;
            }
        }

        #endregion

        #region Internal Methods

        internal static Image GetImage(string s)
        {
            Image img = null;
            try
            {
                ResourceManager rm = new ResourceManager("Futura.ArcGIS.NetworkExtension.Properties.Resources", Assembly.GetExecutingAssembly());
                img = rm.GetObject(s) as Image;
                if (img == null)
                    img = rm.GetObject("exclamation") as Image;
            }
            catch
            {
            }
            return img;
        }

        #endregion
    }

    public enum EditorState
    {
        esriStateNotEditing,
        esriStateEditing,
        esriStateEditingUnfocused
    }
}
