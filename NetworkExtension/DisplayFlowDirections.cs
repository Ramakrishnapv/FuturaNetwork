using ESRI.ArcGIS.Carto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;


namespace Futura.ArcGIS.NetworkExtension
{
    public class DisplayFlowDirections
    {
        private IActiveView _activeView = null;
        private IMap _map = null;

        private IFeatureLayer _primaryLyr = null;
        private IFeatureLayer _secondaryLyr = null;
        private IFeatureRenderer _oldPrimaryRenderer = null;
        private IFeatureRenderer _oldSecRenderer = null;
         
 
        public DisplayFlowDirections(IMap map, IActiveView actView)
        {
            if (map != null)
            {
                this._map = map;
                _activeView = actView;
                _primaryLyr = DisplayMap.GetMapLayerByLayerName(map, "PrimaryConductor", LayerType.FeatureLayer) as IFeatureLayer;
                _secondaryLyr = DisplayMap.GetMapLayerByLayerName(map, "SecondaryConductor", LayerType.FeatureLayer) as IFeatureLayer;
                //this.UpdateActiveViewEvents(actView);
                //((IMapControlEvents2_Event)mapControl).OnMapReplaced += new IMapControlEvents2_OnMapReplacedEventHandler(SourceTab_OnMapReplaced);
            }
        }

        #region Public Methods

        //public bool DisplayPrimaryFlowDirections(ref Dictionary<string, SectionObject> sectionElements, ref Dictionary<string, NodeObject> nElements)
        public bool DisplayPrimaryFlowDirections()
        {
            bool isSuccess = true;
            IGeoFeatureLayer geoFeatLayer = null;
            try
            {
                if (this._primaryLyr != null && this._primaryLyr is IGeoFeatureLayer)
                {
                    geoFeatLayer = this._primaryLyr as IGeoFeatureLayer;
                    if (geoFeatLayer != null)
                    {
                        IFeatureRenderer featRenderer = geoFeatLayer.Renderer;
                        IObjectCopy objCopy = new ObjectCopyClass();
                        //this._oldfeatRenderer = objCopy.Copy(featRenderer) as IFeatureRenderer;
                        this._oldPrimaryRenderer = geoFeatLayer.Renderer;
                        //if (priFlowDirs != null)
                       // {
                            FlowDirection flowDirection = new FlowDirection();
                            flowDirection.FeatureRenderer = this._oldPrimaryRenderer;
                            flowDirection.FeatureSelection = this._primaryLyr as IFeatureSelection;
                            //Commented it because I am doing it inside Renderer class
                            // flowDirection.SelectedOIDs = this.SelectedOIDs(flowDirection.FeatureSelection);
                           // flowDirection.SectionElements = sectionElements;
                            //flowDirection.NodeElements = nElements;
                            //flowDirection.PrimaryFlowDirections = priFlowDirs;
                            geoFeatLayer.Renderer = flowDirection;
                            this._activeView.ContentsChanged();
                            this._activeView.Refresh();
                            isSuccess = true;
                       // }
                    }
                }
                else
                    isSuccess = false;
            }
            catch (Exception ex)
            {
                if (this._oldPrimaryRenderer != null && geoFeatLayer != null)
                {
                    geoFeatLayer.Renderer = this._oldPrimaryRenderer;
                    this._activeView.ContentsChanged();
                    this._activeView.Refresh();
                }
                isSuccess = false;
            }
            finally
            {
                //if (utilityNetwork != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(utilityNetwork);
            }
            return isSuccess;
        }

       // public bool DisplaySecondaryFlowDirections(ref Dictionary<string, SectionObject> sectionElements, ref Dictionary<string, NodeObject> nElements)
         public bool DisplaySecondaryFlowDirections()
        {
            bool isSuccess = true;
            IGeoFeatureLayer geoFeatLayer = null;
            try
            {
                if (this._secondaryLyr != null && this._secondaryLyr is IGeoFeatureLayer)
                {
                    geoFeatLayer = this._secondaryLyr as IGeoFeatureLayer;
                    if (geoFeatLayer != null)
                    {
                        IFeatureRenderer featRenderer = geoFeatLayer.Renderer;
                        IObjectCopy objCopy = new ObjectCopyClass();
                        //this._oldfeatRenderer = objCopy.Copy(featRenderer) as IFeatureRenderer;
                        this._oldSecRenderer = geoFeatLayer.Renderer;
                       // if (secFlowDirs != null)
                       // {
                            FlowDirection flowDirection = new FlowDirection();
                            flowDirection.FeatureRenderer = this._oldSecRenderer;
                            flowDirection.FeatureSelection = this._secondaryLyr as IFeatureSelection;
                            //Commented it because I am doing it inside Renderer class
                            // flowDirection.SelectedOIDs = this.SelectedOIDs(flowDirection.FeatureSelection);
                            //flowDirection.SectionElements = sectionElements;
                            //flowDirection.NodeElements = nElements;
                           // flowDirection.SecFlowDirections = secFlowDirs;
                            geoFeatLayer.Renderer = flowDirection;
                            this._activeView.ContentsChanged();
                            this._activeView.Refresh();
                            isSuccess = true;
                       // }
                    }
                }
                else
                    isSuccess = false;
            }
            catch (Exception ex)
            {
                if (this._oldSecRenderer != null && geoFeatLayer != null)
                {
                    geoFeatLayer.Renderer = this._oldSecRenderer;
                    this._activeView.ContentsChanged();
                    this._activeView.Refresh();
                }
                isSuccess = false;
            }
            finally
            {
                //if (utilityNetwork != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(utilityNetwork);
            }
            return isSuccess;
        }

        private IList<int> SelectedOIDs(IFeatureSelection featSelection)
        {
            IList<int> oids = new List<int>();
            if (featSelection != null && featSelection.SelectionSet != null && featSelection.SelectionSet.Count > 0)
            {
                IEnumIDs ids = default(IEnumIDs);
                int id = -1;
                try
                {
                    ids = featSelection.SelectionSet.IDs;
                    ids.Reset();
                    id = ids.Next();
                    while (!(id == -1))
                    {
                        if (oids.Contains(id) == false)
                            oids.Add(id);

                        id = ids.Next();
                    }
                }
                catch (Exception ex)
                {
                   // EngineLogger.Logger.LogException(ex);
                }
            }
            return oids;
        }

        public void ClearPrimaryFlowDirections()
        {
            if (this._primaryLyr != null && this._primaryLyr is IGeoFeatureLayer)
            {
                IGeoFeatureLayer geoFeatLayer = this._primaryLyr as IGeoFeatureLayer;
                if (geoFeatLayer != null && this._oldPrimaryRenderer != null)
                {
                    geoFeatLayer.Renderer = this._oldPrimaryRenderer;
                    this._activeView.ContentsChanged();
                    this._activeView.Refresh();
                }
            }
        }

        public void ClearSecondaryFlowDirections()
        {
            if (this._secondaryLyr != null && this._secondaryLyr is IGeoFeatureLayer)
            {
                IGeoFeatureLayer geoFeatLayer = this._secondaryLyr as IGeoFeatureLayer;
                if (geoFeatLayer != null && this._oldSecRenderer != null)
                {
                    geoFeatLayer.Renderer = this._oldSecRenderer;
                        this._activeView.ContentsChanged();
                        this._activeView.Refresh();
                }
            }
        }
        #endregion


    }

    public class FlowDirection : IFeatureRenderer
    {

        #region "COM GUIDs"
        // These GUIDs provide the COM identity for this class 
        // and its COM interfaces. If you change them, existing 
        // clients will no longer be able to access the class. 
        public const string ClassId = "4be9936f-3f66-4082-920a-9e12706cc359";
        public const string InterfaceId = "a8a703f4-2ac5-43fe-9bbd-4affc1c7c31c";
        public const string EventsId = "6bbbe967-0ee0-4b4e-b905-3fc9b7a2d86a";
        #endregion

        private IFeatureRenderer rend;
        private IFeatureSelection _featureLayer;
        private int id;
        private IList<int> selectedOids;

        public IFeatureRenderer FeatureRenderer
        {
            get { return rend; }
            set { rend = value; }
        }

        public IFeatureSelection FeatureSelection
        {
            get { return _featureLayer; }
            set { _featureLayer = value; }
        }

        public IList<int> SelectedOIDs
        {
            get { return selectedOids; }
            set { selectedOids = value; }
        }

        // A creatable COM class must have a Public Sub New() 
        // with no parameters, otherwise, the class will not be 
        // registered in the COM registry and cannot be created 
        // via CreateObject. 
        public FlowDirection()
            : base()
        {
        }

        public bool CanRender(ESRI.ArcGIS.Geodatabase.IFeatureClass featClass, ESRI.ArcGIS.Display.IDisplay Display)
        {
            return (featClass.ShapeType == esriGeometryType.esriGeometryPoint);
        }

        public void Draw(ESRI.ArcGIS.Geodatabase.IFeatureCursor cursor, ESRI.ArcGIS.esriSystem.esriDrawPhase DrawPhase, ESRI.ArcGIS.Display.IDisplay Display, ESRI.ArcGIS.esriSystem.ITrackCancel TrackCancel)
        {
            bool bContinue = true;
            IFeature pFeat = null;
            IFeatureDraw pFeatDraw = null;
            ISymbol pFeatSymbol = null;
            try
            {
                if (FeatureSelection != null)
                    this.SelectedOIDs = this.UpdateSelectedOIDs(FeatureSelection);

                pFeat = cursor.NextFeature();
                while (((pFeat != null) && bContinue == true))
                {
                    pFeatDraw = pFeat as IFeatureDraw;
                    pFeatSymbol = GetFeatureSymbol(pFeat);
                    Display.SetSymbol(pFeatSymbol);
                    pFeatDraw.Draw(DrawPhase, Display, pFeatSymbol, true, null, esriDrawStyle.esriDSNormal);
                    pFeat = cursor.NextFeature();
                    if ((TrackCancel != null))
                    {
                        bContinue = TrackCancel.Continue();
                    }
                }
            }
            catch (Exception ex)
            {
               // EngineLogger.Logger.LogException(ex);
            }
            finally
            {
                bContinue = false;
                pFeat = null;
                pFeatDraw = null;
                pFeatSymbol = null;
            }
        }

        public ESRI.ArcGIS.Carto.IFeatureIDSet ExclusionSet
        {
            set { }
        }

        public void PrepareFilter(ESRI.ArcGIS.Geodatabase.IFeatureClass fc, ESRI.ArcGIS.Geodatabase.IQueryFilter queryFilter)
        {
            queryFilter.AddField(fc.OIDFieldName);

            if (this.FeatureRenderer is IUniqueValueRenderer)
            {
                IUniqueValueRenderer uniqueValueRen = FeatureRenderer as IUniqueValueRenderer;
                for (int i = 0; i < uniqueValueRen.FieldCount; i++)
                {
                    queryFilter.AddField(uniqueValueRen.get_Field(i));
                }
            }
        }


        #region IFeatureRenderer Members


        public bool get_RenderPhase(esriDrawPhase DrawPhase)
        {
            return (DrawPhase == esriDrawPhase.esriDPGeography) | (DrawPhase == esriDrawPhase.esriDPAnnotation);
        }

        public ISymbol get_SymbolByFeature(IFeature Feature)
        {
            return this.GetFeatureSymbol(Feature);
        }

        #endregion

        #region Private Methods
        private IList<int> UpdateSelectedOIDs(IFeatureSelection featSelection)
        {
            IList<int> oids = new List<int>();
            if (featSelection != null && featSelection.SelectionSet != null && featSelection.SelectionSet.Count > 0)
            {
                IEnumIDs ids = default(IEnumIDs);
                int id = -1;
                try
                {
                    ids = featSelection.SelectionSet.IDs;
                    ids.Reset();
                    id = ids.Next();
                    while (!(id == -1))
                    {
                        if (oids.Contains(id) == false)
                            oids.Add(id);

                        id = ids.Next();
                    }
                }
                catch (Exception ex)
                {
                    //EngineLogger.Logger.LogException(ex);
                }
            }
            return oids;
        }
        private bool IsSelected(int oid)
        {
            if (this.SelectedOIDs != null && this.SelectedOIDs.Contains(oid))
                return true;
            else
                return false;
        }

        private ISymbol GetFeatureSymbol(IFeature Feature)
        {
            ICartographicLineSymbol hashsym = null;
            IRgbColor pcolor = null;
            ILineSymbol lsym = null;
           // ISimpleEdgeFeature seFeat = null;
            ILineProperties lineProperties = null;
            IArrowMarkerSymbol aMSymbol = null;
            ILineDecoration lineDecoration = null;
            ISimpleLineDecorationElement simpleLineDecorationElement = null;
            IMarkerSymbol smSym = null;
            bool isMultiLyrSym = false;
            int multiLyrSymIndex = -1;
            try
            {
                if (Feature.HasOID)
                {
                    int status = 0;
                    if (string.Compare(Feature.Class.AliasName.Substring(Feature.Class.AliasName.LastIndexOf('.') + 1), "PrimaryConductor", true) == 0)
                    {
                        if (ExtensionInfo.netUtil.primaryStatuses.ContainsKey(Feature.OID))
                            status = ExtensionInfo.netUtil.primaryStatuses[Feature.OID];
                    }
                    else if (ExtensionInfo.netUtil.secondaryStatuses.ContainsKey(Feature.OID))
                        status = ExtensionInfo.netUtil.secondaryStatuses[Feature.OID];

                       //int fldIndexGUID = Feature.Fields.FindField("FuturaGUID");
                       //if (fldIndexGUID != -1)
                       //{
                       //    IFeatureClass featCls = Feature.Class as IFeatureClass;
                       //    IFeature feat = featCls.GetFeature(Feature.OID);

                       //    string guid = feat.get_Value(fldIndexGUID).ToString();
                       //    if (guid != null)
                       //        status = (int)NetworkExtension.FuturaNetworkExt.netUtil._sectionList[guid].status;
                       //}

                   // int status = NetworkExtension.FuturaNetworkExt.netUtil._sectionList.Values.FirstOrDefault(obj => (obj.objectId == Feature.OID && string.Compare(Feature.Class.AliasName.Substring(Feature.Class.AliasName.LastIndexOf('.')+1), obj.className, true) == 0)).status;

                   // string guid = Feature.get_Value(Feature.Fields.FindField("FuturaGUID")).ToString();
                   // int status = NetworkExtension.FuturaNetworkExt.netUtil._sectionList[guid].status;
                    pcolor = new RgbColor();
                    //FeatureRenderer.PrepareFilter(Feature.Class as IFeatureClass, new QueryFilterClass());
                    //below line used to get default linesymbol of a featureclass 
                    lsym = FeatureRenderer.get_SymbolByFeature(Feature) as ILineSymbol;

                    if (lsym is ICartographicLineSymbol)
                        hashsym = lsym as ICartographicLineSymbol;
                    else if (lsym is IMultiLayerLineSymbol)
                    {
                        IMultiLayerLineSymbol multiLyrLineSymbol = lsym as IMultiLayerLineSymbol;
                        for (int i = 0; i < multiLyrLineSymbol.LayerCount; i++)
                        {
                            if (multiLyrLineSymbol.get_Layer(i) != null && multiLyrLineSymbol.get_Layer(i) is ICartographicLineSymbol)
                            {
                                hashsym = multiLyrLineSymbol.get_Layer(i) as ICartographicLineSymbol;
                                isMultiLyrSym = true;
                                multiLyrSymIndex = i;
                                break;
                            }
                        }
                    }

                    if (hashsym == null)
                    {
                        hashsym = new CartographicLineSymbolClass();
                        hashsym.Color = lsym.Color;
                        hashsym.Width = lsym.Width;
                    }
                    if (FeatureSelection.SelectionSet.Count > 0 && IsSelected(Feature.OID))
                    {
                        //if (MobileSettings.settings.UserSettings.TraceSettings.LineColor.R == 0 && MobileSettings.settings.UserSettings.TraceSettings.LineColor.G == 255 && MobileSettings.settings.UserSettings.TraceSettings.LineColor.B == 255)
                        //{
                        //    pcolor.Red = 0;
                        //    pcolor.Green = 255;
                        //    pcolor.Blue = 0;
                        //}
                        //else
                        //{
                            pcolor.Red = 0;
                            pcolor.Green = 255;
                            pcolor.Blue = 255;
                       // }
                        //lsym.Color = pcolor;
                        //lsym.Width = lsym.Width + 1;
                        hashsym.Color = pcolor;
                        hashsym.Width = hashsym.Width + 1;
                    }
                    else
                    {
                        //pcolor.Red = MobileSettings.settings.UserSettings.TraceSettings.LineColor.R;
                        //pcolor.Green = MobileSettings.settings.UserSettings.TraceSettings.LineColor.G;
                        //pcolor.Blue = MobileSettings.settings.UserSettings.TraceSettings.LineColor.B;
                        //lsym.Color = pcolor;
                    }

                    if (Feature.Shape.GeometryType == esriGeometryType.esriGeometryLine || Feature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                    {
                       // SectionObject sObj = null;
                       // Orientation dir = Orientation.Uninitialized;
                       // seFeat = Feature as ISimpleEdgeFeature;
                      //   int fldIndexGUID = Feature.Fields.FindField(((IClassEx)Feature.Class).GlobalIDFieldName);
                       // if(fldIndexGUID != -1)
                      //  {
                            //IFeatureClass featCls = Feature.Class as IFeatureClass;
                            //IFeature feat = featCls.GetFeature(Feature.OID);

                            //object guid = feat.get_Value(fldIndexGUID);
                            // if (guid != null)
                            //    sObj = secElements.ContainsKey(guid.ToString()) ? secElements[guid.ToString()] : null;

                            //if(sObj != null)
                            //{
                            //    IPolyline pl = Feature.ShapeCopy as IPolyline;
                            //    NodeObject fNode = nodeElements.ContainsKey(sObj.Parent) ? nodeElements[sObj.Parent] : null;
                            //    NodeObject tNode = nodeElements.ContainsKey(sObj.Child) ? nodeElements[sObj.Child] : null;
                            //    if(fNode != null && tNode != null)
                            //    {
                            //        if (Math.Abs(fNode.X - pl.FromPoint.X) < 0.001 && Math.Abs(fNode.Y - pl.FromPoint.Y) < 0.001)
                            //            dir = Orientation.WithFlow;
                            //        else
                            //            dir = Orientation.AgainstFlow;
     
                            //    }
                            //}
                       // }

                        //bool isPrimary = string.Compare(Feature.Class.AliasName, "PrimaryConductor", true) == 0;
                        //try
                        //{
                        //    if (isPrimary && PrimaryFlowDirections != null)
                        //        dir = PrimaryFlowDirections[Feature.OID];
                        //    // dir = secElements.Values.FirstOrDefault(obj => (obj.OID == Feature.OID && string.Compare(Feature.Class.AliasName, obj.LayerName, true) == 0));
                        //    else if (SecFlowDirections != null)
                        //        dir = SecFlowDirections[Feature.OID];
                        //}
                        //catch
                        //{
                        //    dir = Orientation.Uninitialized;
                        //}
                        //if (sObj != null)
                       // {
                           // dir = sObj.FlowDirection;
                                //IPolyline pl = Feature.ShapeCopy as IPolyline;
                                //NodeObject fNode = nodeElements.ContainsKey(sObj.Parent) ? nodeElements[sObj.Parent] : null;
                                //NodeObject tNode = nodeElements.ContainsKey(sObj.Child) ? nodeElements[sObj.Child] : null;
                                //if (fNode != null && tNode != null)
                                //{
                                //    bool isUninitialized = false;
                                //    if (string.Compare(fNode.LayerName, FuturaNetJunctions.Futura_NetJunctions_TableName, true) == 0 && secElements.Values.FirstOrDefault(obj => (obj.Child == fNode.GUID)) == null)
                                //        dir = Orientation.Indeterminate;
                                //    else
                                //    {
                                //        if (Math.Abs(fNode.X - pl.FromPoint.X) < 0.001 && Math.Abs(fNode.Y - pl.FromPoint.Y) < 0.001)
                                //            dir = Orientation.WithFlow;
                                //        else
                                //            dir = Orientation.AgainstFlow;
                                //    }
                                //}
                          
                       // }

                        lineProperties = hashsym as ILineProperties;
                        simpleLineDecorationElement = new SimpleLineDecorationElement();
                        if (FuturaNetwork.StatusExtensions.IsLoop(status))
                            simpleLineDecorationElement.MarkerSymbol = DefaultUninitializedSymbol();
                        else if (FuturaNetwork.StatusExtensions.IsEnergized(status))
                        {
                            //if (smSym == null)
                            //    smSym = Global.DefaultFlowSymbol();

                            if (FuturaNetwork.StatusExtensions.BreakDown(status).Contains(FuturaNetwork.Constants.WithFlow))
                                simpleLineDecorationElement.MarkerSymbol = DefaultFlowSymbol(false);
                            else
                                simpleLineDecorationElement.MarkerSymbol = DefaultFlowSymbol(true);

                        }
                        else
                            simpleLineDecorationElement.MarkerSymbol = DefaultUninitializedSymbol();

                        simpleLineDecorationElement.AddPosition(0.5);
                        simpleLineDecorationElement.PositionAsRatio = true;
                        lineDecoration = new LineDecoration();
                        lineDecoration.AddElement(simpleLineDecorationElement);
                        lineProperties.LineDecoration = lineDecoration;
                    }

                    ISymbol featSymbol = null;

                    if (isMultiLyrSym && multiLyrSymIndex != -1)
                    {
                        IMultiLayerLineSymbol multiLyrLineSymbol = lsym as IMultiLayerLineSymbol;
                        if (multiLyrLineSymbol.get_Layer(multiLyrSymIndex) != null && multiLyrLineSymbol.get_Layer(multiLyrSymIndex) is ICartographicLineSymbol)
                        {
                            ILineSymbol lineSymbol = multiLyrLineSymbol.get_Layer(multiLyrSymIndex);
                            multiLyrLineSymbol.DeleteLayer(lineSymbol);
                            multiLyrLineSymbol.AddLayer(hashsym as ILineSymbol);
                        }
                        featSymbol = multiLyrLineSymbol as ISymbol;
                    }
                    else
                        featSymbol = hashsym as ISymbol;

                    return featSymbol;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                //EngineLogger.Logger.LogException(ex);
                return null;
            }
            finally
            {
                hashsym = null;
                pcolor = null;
                lsym = null;
                //seFeat = null;
                lineProperties = null;
                aMSymbol = null;
                lineDecoration = null;
                simpleLineDecorationElement = null;
                smSym = null;
            }
        }


        public static IMarkerSymbol DefaultFlowSymbol(bool reverse)
        {
            IArrowMarkerSymbol arrowMarkerSymbol = new ArrowMarkerSymbolClass();
            arrowMarkerSymbol.Size = 12;
            if (reverse) arrowMarkerSymbol.Angle = 180;
            return arrowMarkerSymbol;
        }

        public static IMarkerSymbol DefaultUninitializedSymbol()
        {
            ISimpleMarkerSymbol uninitializedSymbol = new SimpleMarkerSymbolClass();
            uninitializedSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
            return uninitializedSymbol;
        }

        public static IMarkerSymbol DefaultIndeterminateSymbol()
        {
            ISimpleMarkerSymbol indeterminateSymbol = new SimpleMarkerSymbolClass();
            indeterminateSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
            return indeterminateSymbol;
        }
        #endregion


    }

}
