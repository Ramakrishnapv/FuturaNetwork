using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UndoRedoFramework;
using FuturaNetwork;
using ESRI.ArcGIS.Geometry;

namespace Futura.ArcGIS.NetworkExtension
{
    public class WorkspaceListener
    {
        public WorkspaceListener(IEditor3 editor)
        {
            // IEditEvents3_SinkHelper ev = editor as IEditEvents3_SinkHelper;
            //ev.OnStartEditing +=
            IEditEvents_Event ev = editor as IEditEvents_Event;
            IEditEvents2_Event ev2 = editor as IEditEvents2_Event;

            ev.OnStartEditing += WorkspaceListener_OnStartEditing;
            ev.OnStopEditing += ev_OnStopEditing;
            ev.OnCreateFeature += ev_OnCreateFeature;
            ev.OnChangeFeature += ev_OnChangeFeature;
            ev.OnDeleteFeature += ev_OnDeleteFeature;
            ev.OnUndo += ev_OnUndo;
            ev.OnRedo += ev_OnRedo;
            // ev2.OnStartOperation += WorkspaceListener_OnStartOperation;
            //ev2.OnStopOperation += WorkspaceListener_OnStopOperation;
            //((IEditEvents_Event)editor).OnStartEditing += WorkspaceListener_OnStartEditing;
        }


        void WorkspaceListener_OnStopOperation()
        {
            //SRIRAM:commit undoredo
            UndoRedoManager.Commit();
            if (this.itIsInvalidOperation) //undo invalid edit operation
            {
                ((IWorkspaceEdit2)ExtensionInfo.Editor.EditWorkspace).UndoEditOperation();
                this.itIsInvalidOperation = false;
            }
        }

        void WorkspaceListener_OnStartOperation()
        {
            //SRIRAM:start undoredo
            UndoRedoManager.Start("start operation");
        }

        void ev_OnRedo()
        {
            //SRIRAM:undoredo.redo
            if (UndoRedoManager.CanRedo)
                UndoRedoManager.Redo();
        }

        void ev_OnUndo()
        {
            //SRIRAM:undoredo.undo
            if (UndoRedoManager.CanUndo)
                UndoRedoManager.Undo();
        }

        void ev_OnDeleteFeature(IObject obj)
        {

        }

        private bool itIsInvalidOperation = false;
        void ev_OnChangeFeature(IObject obj)
        {
            this.itIsInvalidOperation = false;
            IRow row = obj as IRow;
            IFeature feature = obj as IFeature;
            ElementType elementType = ElementType.None;
            string guid = string.Empty;
            //changed fields
            IList<string> changedFieldNames = null;
            bool shapeChanged = false;
            bool constPhaseChanged = false;
            bool phaseChanged = false;
            bool statusChanged = false;
            bool sourceChanged = false;
            try
            {

                //ignore if it internal edit
                if (row != null && !ExtensionInfo.InternalEdit)
                {
                    //get GUID
                    guid = DatabaseUtil.ToText(DatabaseUtil.GetFieldValue(row, ExtensionInfo.netUtil.ntInfo.guidName));
                    if (feature != null && ExtensionInfo.netUtil.ntInfo.networkClassIds.ContainsValue(feature.Class.ObjectClassID))
                    {
                        //find element type
                        elementType = (feature.ShapeCopy.GeometryType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint) ? ElementType.Node : ElementType.Section;
                        //get changed fields
                        #region get changed fields
                        changedFieldNames = DisplayMap.GetNamesOfChangedFields(row);
                        foreach (string fieldName in changedFieldNames)
                        {
                            if (string.Compare(fieldName, ExtensionInfo.netUtil.ntInfo.phaseCodeField, true) == 0)
                            {
                                if (elementType == ElementType.Section)
                                {
                                    this.itIsInvalidOperation = true; //undo the edit operation
                                    System.Windows.Forms.MessageBox.Show("Cannot change PhaseCode on sections. Change on device or use Phase update tool.", "Invalid Edit", System.Windows.Forms.MessageBoxButtons.OK);
                                    return;
                                }
                                phaseChanged = true;
                            }
                            else if (string.Compare(fieldName, ExtensionInfo.netUtil.ntInfo.constructedPhaseField, true) == 0)
                            {
                                constPhaseChanged = true;
                            }
                            else if (feature != null)
                            {
                                if (string.Compare(fieldName, ((IFeatureClass)feature.Class).ShapeFieldName, true) == 0
                                    || (elementType == ElementType.Section && (string.Compare(fieldName, ((IFeatureClass)feature.Class).LengthField.Name, true)) == 0))
                                    shapeChanged = true;
                            }
                            else if (string.Compare(fieldName, ExtensionInfo.netUtil.ntInfo.statusField, true) == 0)
                                statusChanged = true;
                            else if (ExtensionInfo.netUtil.ntInfo.Source != null && ExtensionInfo.netUtil.ntInfo.Source.ClassID == feature.Class.ObjectClassID)
                            {
                                if (string.IsNullOrEmpty(ExtensionInfo.netUtil.ntInfo.Source.FieldName) ||
                                    ExtensionInfo.netUtil.ntInfo.Source.FieldValue == DatabaseUtil.GetFieldValue(row, ExtensionInfo.netUtil.ntInfo.Source.FieldName))
                                    sourceChanged = true;
                            }
                        }
                        #endregion

                        //SRIRAM:when updating downsteam features, set InternalEdit = true & UpdateConnectivity = false
                        #region constructionPhase updated
                        if (constPhaseChanged)
                        {

                        }
                        #endregion

                        #region phase updated
                        if (phaseChanged)
                        {
                        }
                        #endregion

                        #region status changed
                        if (statusChanged)
                        {
                            int enabled = DatabaseUtil.ToInteger(DatabaseUtil.GetFieldValue(row, ExtensionInfo.netUtil.ntInfo.statusField), -1);// Convert.ToInt32(feature.get_Value(feature.Fields.FindField(enabledFieldName)));
                            if (enabled == 0) //Enabled => Disabled
                            {

                            }
                            else if (enabled == 1) //Disabled => Enabled
                            {

                            }
                        }
                        #endregion

                        #region source changed
                        if (sourceChanged)
                        {
                        }
                        #endregion

                        #region shape updated
                        if (shapeChanged)
                        {
                        }
                        #endregion

                        //SRIRAM: update element in FTA_COnnectivity, FTA_Elements unless it is invalid edit
                    }
                    else
                    {
                        //run autonumber
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }

        public void AddSecondaryToAdjNode(Node nd, Node adjNd)
        {
            foreach (Section sec in nd.childList.Values)
            {
                adjNd.childList = new Dictionary<string, Section>();
                if (ClassIDRules.AdjRuleSucceeds(sec))
                {
                    adjNd.childList.Add(sec.uid, sec);
                }
            }
        }

        public void AdjustParentChild(Node nd, Node newNd)
        {
            foreach (Section sec in nd.childList.Values)
            {
                sec.parentNode = newNd;
            }
            foreach (Section sec in nd.parentList.Values)
            {
                sec.childNode = newNd;
            }
        }

        public void AddNode(EditObject eObj)
        {
            Node snappednd = null;
            Node ndNew = null;
            if (eObj.Connectivity.snappedNodes.Count > 1)
            {
                snappednd = eObj.Connectivity.snappedNodes.First(nd => StatusExtensions.IsNetJunction(nd.status));
            }
            snappednd = eObj.Connectivity.snappedNodes != null ? eObj.Connectivity.snappedNodes[0] : null;
            if (StatusExtensions.IsNetJunction(snappednd.status) && ClassIDRules.IsAdjJunction(snappednd) && ClassIDRules.IsAdjGroupNode(eObj.CLSID))
            {
                ndNew = new Node();
                ndNew.status = eObj.Connectivity.status;
                ndNew.uid = eObj.UID;
                snappednd.adjacentNode = ndNew;
                ndNew.classID = eObj.CLSID;
                ndNew.x = snappednd.x;
                ndNew.y = snappednd.y;
                ndNew.phaseCode = eObj.Connectivity.phaseCode;
                ndNew.parentList = snappednd.parentList;
                AddSecondaryToAdjNode(snappednd, ndNew);                
            }
            else if (StatusExtensions.IsNetJunction(snappednd.status))
            {
                ndNew = new Node();
                ndNew.status = eObj.Connectivity.status;
                ndNew.uid = eObj.UID;
                ndNew.x = snappednd.x;
                ndNew.y = snappednd.y;
                ndNew.classID = eObj.CLSID;
                ndNew.phaseCode = eObj.Connectivity.phaseCode;
                ndNew.parentList = snappednd.parentList;
                ndNew.childList = snappednd.childList;
                AdjustParentChild(snappednd, ndNew);
                //now delete net junction *****
                if (StaticStuff._nodeList.ContainsKey(snappednd.uid)) StaticStuff._nodeList.Remove(snappednd.uid);
            }
            if (ndNew != null)
            {
                StaticStuff._nodeList.Add(ndNew.uid, ndNew);
                ExtensionInfo.checkFlow.AddPointsToPointList(ndNew);
            }
        }

        public Node CreateNetJn(EditObject eObj)
        {
            Node netJn = new Node();
            netJn.status = Constants.Energized + Constants.NetJunction;
            if (StatusExtensions.IsUnenergized(eObj.Connectivity.status))
            {
                netJn.status = Constants.Unenergized + Constants.NetJunction;
            }
            else if (StatusExtensions.IsEnergized(eObj.Connectivity.status))
            {
                netJn.status = Constants.Energized + Constants.NetJunction;
            }
            else if (StatusExtensions.IsLoop(eObj.Connectivity.status))
            {
                netJn.status = Constants.Loop + Constants.NetJunction;
            }
            if (StatusExtensions.IsDisconnected(eObj.Connectivity.status))
            {
                netJn.status = Constants.Disconnected + Constants.NetJunction;
            }
            netJn.uid = "{" + Guid.NewGuid().ToString() + "}";
            netJn.phaseCode = eObj.Connectivity.phaseCode;
            netJn.classID = -1;//ntInfo.networkClassIds[ClassIDRules.NetJunction];
            netJn.oid = -1;
            return netJn;
        }

        public void AddSection(EditObject eObj)
        {
            Section newSect = new Section();
            newSect.uid = eObj.UID;
            if (eObj.Connectivity.parentNode == null)
            {
                IPolyline ln = (IPolyline)eObj.shape;
                Node FromNetJn = CreateNetJn(eObj);
                FromNetJn.x = ln.FromPoint.X;
                FromNetJn.y = ln.FromPoint.Y;
                eObj.Connectivity.parentNode = FromNetJn;
                StaticStuff._nodeList.Add(FromNetJn.uid, FromNetJn);
                ExtensionInfo.checkFlow.AddPointsToPointList(FromNetJn);
                Node toNetJn = CreateNetJn(eObj);
                toNetJn.x = ln.ToPoint.X;
                toNetJn.y = ln.ToPoint.Y;
                eObj.Connectivity.childNode = toNetJn;
                StaticStuff._nodeList.Add(toNetJn.uid, toNetJn);
                ExtensionInfo.checkFlow.AddPointsToPointList(toNetJn);
            }
            else if (eObj.Connectivity.childNode == null)
            {
                Node netJn = CreateNetJn(eObj);
                IPolyline ln = (IPolyline)eObj.shape;
                if (StatusExtensions.IsWithFlow(eObj.Connectivity.status))
                {
                    netJn.x = ln.ToPoint.X;
                    netJn.y = ln.ToPoint.Y;
                }
                else if (StatusExtensions.IsAgainstFlow(eObj.Connectivity.status))
                {
                    netJn.x = ln.FromPoint.X;
                    netJn.y = ln.FromPoint.Y; ;
                }
                eObj.Connectivity.childNode = netJn;
                StaticStuff._nodeList.Add(netJn.uid, netJn);
                ExtensionInfo.checkFlow.AddPointsToPointList(netJn);
            }
            newSect.phaseCode = eObj.Connectivity.phaseCode;
            newSect.classID = eObj.CLSID;
            newSect.status = eObj.Connectivity.status;
            if (StatusExtensions.IsWithFlow(eObj.Connectivity.status))
            {
                newSect.x = eObj.Connectivity.parentNode.x;
                newSect.y = eObj.Connectivity.parentNode.y;
                newSect.tox = eObj.Connectivity.childNode.x;
                newSect.toy = eObj.Connectivity.childNode.y;
            }
            else if (StatusExtensions.IsAgainstFlow(eObj.Connectivity.status))
            {
                newSect.x = eObj.Connectivity.childNode.x;
                newSect.y = eObj.Connectivity.childNode.y;
                newSect.tox = eObj.Connectivity.parentNode.x;
                newSect.toy = eObj.Connectivity.parentNode.y;
            }

            newSect.classID = eObj.CLSID;
            newSect.phaseCode = eObj.Connectivity.phaseCode;
            newSect.parentNode = eObj.Connectivity.parentNode;
            newSect.childNode = eObj.Connectivity.childNode;
            if (newSect.parentNode.childList == null)
                newSect.parentNode.childList = new Dictionary<string, Section>();
            newSect.parentNode.childList.Add(newSect.uid, newSect);
            if (newSect.childNode.parentList == null)
                newSect.childNode.parentList = new Dictionary<string, Section>();
            newSect.childNode.parentList.Add(newSect.uid, newSect);

            StaticStuff._sectionList.Add(newSect.uid, newSect);
            ExtensionInfo.checkFlow.AddPointsToLineList(newSect);
        }

        void ev_OnCreateFeature(IObject obj)
        {
            //check if it is internal edit. If new feature created with insertion tool, it will be internal tool
            if (!ExtensionInfo.InternalEdit)
            {
                //SRIRAM:get parent, inherit from parent, autonumber etc
            }

            EditObject eObj = ExtensionInfo.EditQueue.FirstOrDefault(eo => (eo.OID == obj.OID && eo.CLSID == obj.Class.ObjectClassID));
            if (eObj != null)
            {

                if (ExtensionInfo.UpdateConnectivity)
                {
                    if (eObj.ElementType == ElementType.Node)
                    {
                        AddNode(eObj);

                        //SRIRAM:create node in FTA_Elements table

                        //SRIRAM:update connected sections in FTA_Connectivity; Update PN or CN.Update node status, ph, classid. Only snapped node is NetJn. 

                        //SRIRAM:delete netJn from FTA_Elements if snapped nodes has NetJn

                    }
                    else
                    {
                        AddSection(eObj);
                        //SRIRAM:create a section in FTA_Elements table. The status will be EditObject.EditObjConnectivity.Status


                        //SRIRAM:Add it to FTA_Connectivity and create NetJn(s) if PN or CN missing

                        if (eObj.Connectivity.setFlow) //SRIRAM:setflow downstream
                        {

                        }

                        //SRIRAM:update flowdirections collection
                    }
                }

                //remove the edit object from list
                ExtensionInfo.EditQueue.Remove(eObj);
            }
        }

        void ev_OnStopEditing(bool save)
        {

            if (save)
            {
                //SRIRAM:update network changes to database
            }
            else
            {
                while (UndoRedoManager.CanUndo)
                {
                    UndoRedoManager.Undo();
                }
                //SRIRAM:discard all changes
            }

            //SRIRAM:clear undoredo stack
            UndoRedoManager.FlushHistory();
            //unreg ws edit events
            if (_wsEditEventsRegistered && ExtensionInfo.Editor.EditWorkspace != null)
            {
                IWorkspaceEditEvents_Event workspaceEditEvents = ExtensionInfo.Editor.EditWorkspace as IWorkspaceEditEvents_Event;
                if (workspaceEditEvents != null)
                {
                    // workspaceEditEvents.OnAbortEditOperation += new IWorkspaceEditEvents_OnAbortEditOperationEventHandler(_WorkspaceEditEvents_Event_OnAbortEditOperation);
                    workspaceEditEvents.OnRedoEditOperation -= ev_OnRedo;// new IWorkspaceEditEvents_OnRedoEditOperationEventHandler(_WorkspaceEditEvents_Event_OnRedoEditOperation);
                    workspaceEditEvents.OnStartEditOperation -= WorkspaceListener_OnStartOperation;// new IWorkspaceEditEvents_OnStartEditOperationEventHandler(_WorkspaceEditEvents_Event_OnStartEditOperation);
                    workspaceEditEvents.OnStopEditOperation -= WorkspaceListener_OnStopOperation;// new IWorkspaceEditEvents_OnStopEditOperationEventHandler(_WorkspaceEditEvents_Event_OnStopEditOperation);
                    workspaceEditEvents.OnUndoEditOperation -= ev_OnUndo;// new IWorkspaceEditEvents_OnUndoEditOperationEventHandler(_WorkspaceEditEvents_Event_OnUndoEditOperation);
                }
                _wsEditEventsRegistered = false;
            }

        }

        private bool _wsEditEventsRegistered = false;
        public void WorkspaceListener_OnStartEditing()
        {
            IList<ILayer> mapLayers = DisplayMap.GetMapLayersByLayerType(ArcMap.Document.FocusMap, LayerType.GeoFeatureLayer, true);
            if (mapLayers != null)
            {
                foreach (ILayer lyr in mapLayers)
                {
                    if (((IFeatureLayer)lyr).FeatureClass != null)
                    {
                        string name = DisplayMap.GetQualifiedName(((IFeatureLayer)lyr).FeatureClass);
                        if (ExtensionInfo.FeatureLyrByFCName.ContainsKey(name) == false)
                            ExtensionInfo.FeatureLyrByFCName.Add(name, lyr as IFeatureLayer);
                    }
                }
            }

            //register WS edit events
            if (ExtensionInfo.Editor.EditWorkspace != null)
            {
                IWorkspaceEditEvents_Event workspaceEditEvents = ExtensionInfo.Editor.EditWorkspace as IWorkspaceEditEvents_Event;
                if (workspaceEditEvents != null && !_wsEditEventsRegistered)
                {
                    workspaceEditEvents.OnAbortEditOperation += workspaceEditEvents_OnAbortEditOperation;
                    workspaceEditEvents.OnRedoEditOperation += ev_OnRedo;// new IWorkspaceEditEvents_OnRedoEditOperationEventHandler(_WorkspaceEditEvents_Event_OnRedoEditOperation);
                    workspaceEditEvents.OnStartEditOperation += WorkspaceListener_OnStartOperation;// new IWorkspaceEditEvents_OnStartEditOperationEventHandler(_WorkspaceEditEvents_Event_OnStartEditOperation);
                    workspaceEditEvents.OnStopEditOperation += WorkspaceListener_OnStopOperation;// new IWorkspaceEditEvents_OnStopEditOperationEventHandler(_WorkspaceEditEvents_Event_OnStopEditOperation);
                    workspaceEditEvents.OnUndoEditOperation += ev_OnUndo;// new IWorkspaceEditEvents_OnUndoEditOperationEventHandler(_WorkspaceEditEvents_Event_OnUndoEditOperation);
                }
                _wsEditEventsRegistered = true;
            }

        }

        void workspaceEditEvents_OnAbortEditOperation()
        {
            //SRIRAM:undo undoredo

        }



        private List<int> AutonumberedCLSIDs()
        {
            List<int> clsids = new List<int>();
            //TODO
            return clsids;
        }

        ~WorkspaceListener()
        {

        }
    }
}
