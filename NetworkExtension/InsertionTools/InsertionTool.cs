using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using System.Drawing;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Editor;
using System.Runtime.InteropServices;
using FuturaNetwork;

namespace Futura.ArcGIS.NetworkExtension
{
    public class InsertionTool : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        public string Associated_FeatureClassName = string.Empty;

        private IPoint _vertexPoint;
        private Form _contextMenu;
        private IWorkspace _workspace;
        private IFeature _newFeature;
        private IPoint _initialAnchorPoint;
        private IDisplayFeedback _insertionFeedback;
        private IGeoFeatureLayer _currentTargetLayer;
        private ISegmentCollection _segmentCollection;
        private IMovePointFeedback _movePointFeedback;
        private IPoint _segmentAnchorPoint, _constrainedAnchor;
        private bool _operationStarted, _moveDeviceAlongLineOnInsert, _insertionFeedbackInUse;
        private double _angle, _distance, _constrainedDistance, _constrainedAngle, _absX, _absY;
        private bool _isDistAngleFormDisposed = false;

        #region Public Events
        public event SketchStatusUpdateEventHandler OnSketchUpdate;
        #endregion

        public InsertionTool()
        {
            _movePointFeedback = new MovePointFeedbackClass();
            if (ArcMap.Document.ActiveView != null)
                _movePointFeedback.Display = ArcMap.Document.ActiveView.ScreenDisplay;
            _movePointFeedback.Symbol = Cartography.CreateInsertionSymbol();
        }

        protected override void OnUpdate()
        {
            Enabled = (ExtensionInfo.Extension != null && 
                        ExtensionInfo.Extension.ExtensionState == ESRI.ArcGIS.Desktop.AddIns.ExtensionState.Enabled &&
                        ExtensionInfo.Editor.EditState == ESRI.ArcGIS.Editor.esriEditState.esriStateEditing &&
                        ExtensionInfo.FeatureLyrByFCName.ContainsKey(Associated_FeatureClassName));
        }

        protected override void OnMouseDown(MouseEventArgs arg)
        {

            _absX = arg.X;
            _absY = arg.Y;
            if (arg.Button == MouseButtons.Left)
            {
                _constrainedAngle = 0;
                _constrainedDistance = 0;

                if (_currentTargetLayer != null)
                {
                    if (ArcMap.Document.ActiveView.ScreenDisplay != null)
                    {
                        IPoint anchor = ArcMap.Document.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y);
                        GISApplication.SnapPoint(anchor);

                        if (_currentTargetLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
                        {
                            _initialAnchorPoint = null;
                            try
                            {
                                if (_moveDeviceAlongLineOnInsert)
                                {
                                    if (!arg.Control)
                                    {
                                        //get connected section which is longer
                                        IFeature nearestEdge = null;// GetNearestEdge(anchor, null, new List<string>() { "PrimaryConductor" });// NetworkAnalysis.GetEdgeNearestToPoint(((INetworkClass)_currentTargetLayer.FeatureClass).GeometricNetwork, ArcMap.Document.FocusMap, anchor);
                                        if (nearestEdge != null)
                                        {
                                            if (nearestEdge.Shape is IPolyline)
                                            {
                                                bool rightSide = false;
                                                double distanceAlongCurve = 0, distanceFromCurve = 0;

                                                IPoint outPoint = new PointClass();
                                                IPolyline pLine = nearestEdge.Shape as IPolyline;
                                                pLine.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, anchor, true, outPoint, ref distanceAlongCurve, ref distanceFromCurve, ref rightSide);

                                                bool asRatio = false;
                                                IPoint newAnchor = new PointClass();
                                                double distanceToMoveAlongCurve = 30;// (double)UserDefinedElectricSettings.PrimaryDeviceInsertionOffset;//TODO.configurable

                                                if (pLine.Length < 10) //TODO.configurable
                                                {
                                                    MessageBox.Show("The line you are trying to split is shorter than the " + Environment.NewLine + "[MinimumLineSplitDistanceForPrimaryDevices] setting value of 10", "Invalid Operation.", MessageBoxButtons.OK);
                                                    return;
                                                }

                                                if (pLine.Length <= (distanceToMoveAlongCurve + 1))
                                                {
                                                    distanceToMoveAlongCurve = .3;
                                                    asRatio = true;
                                                }

                                                if (distanceAlongCurve <= .5)
                                                    pLine.QueryPoint(esriSegmentExtension.esriNoExtension, distanceToMoveAlongCurve, asRatio, newAnchor);
                                                else if (distanceAlongCurve > .5)
                                                {
                                                    if (asRatio)
                                                        distanceToMoveAlongCurve = 1 - distanceToMoveAlongCurve;
                                                    else
                                                        distanceToMoveAlongCurve = pLine.Length - distanceToMoveAlongCurve;
                                                    pLine.QueryPoint(esriSegmentExtension.esriNoExtension, distanceToMoveAlongCurve, asRatio, newAnchor);
                                                }

                                                if (DisplayMap.IsGeometryValid(newAnchor))
                                                {
                                                    //Center on the insertion point if it is outside the visible extent.
                                                    if (DisplayMap.IsPointInExtent(newAnchor, ArcMap.Document.ActiveView.Extent) == false)
                                                        DisplayMap.ZoomToPoint(ArcMap.Document.ActiveView, newAnchor);
                                                    anchor = newAnchor;
                                                    _movePointFeedback.MoveTo(anchor);
                                                }
                                            }
                                        }
                                    }
                                }
                                CreateFeature(anchor);
                            }
                            catch(Exception ex)
                            {
                                DeactivateCurrentTool();
                            }
                        }
                        else
                        {
                            if (!_insertionFeedbackInUse)
                            {
                                _segmentCollection = new PolylineClass();
                                _segmentAnchorPoint = null;

                                if (_currentTargetLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                                {
                                    _insertionFeedbackInUse = true;
                                    _insertionFeedback = new NewLineFeedbackClass();
                                    INewLineFeedback lineEditFeedback = (INewLineFeedback)_insertionFeedback;
                                    lineEditFeedback.Start(anchor);
                                    _initialAnchorPoint = anchor;

                                }
                                else if (_currentTargetLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                                {
                                    _insertionFeedbackInUse = true;
                                    _insertionFeedback = new NewLineFeedbackClass();
                                    INewPolygonFeedback polygonEditFeedback = (INewPolygonFeedback)_insertionFeedback;
                                    polygonEditFeedback.Start(anchor);
                                }
                                if (_insertionFeedback != null)
                                    _insertionFeedback.Display = ArcMap.Document.ActiveView.ScreenDisplay;
                            }
                            else//LINES
                            {
                                if (_constrainedAnchor != null)
                                    ProcessSegment(_constrainedAnchor);
                                else
                                    ProcessSegment(anchor);
                            }
                        }
                        anchor.SnapToSpatialReference();
                    }
                }
            }
            else //if (_insertionFeedbackInUse)
                DisplayContextMenu(_currentTargetLayer.FeatureClass.ShapeType);

        }

        public IFeature GetNearestEdge(IPoint point, IPoint otherPt, List<string> edgeSearchClasses) //otherPt is for line features to chck if it is not split
        {
            IGeometry queryGeometry = point;
            IFeature nearestFeat = null;

            foreach (string edgeFeatClass in edgeSearchClasses)
            {
                IFeatureClass featClass = ((IFeatureWorkspace)ExtensionInfo.Editor.EditWorkspace).OpenFeatureClass(edgeFeatClass) as IFeatureClass;
                if (featClass != null)
                {
                    ISpatialFilter spatialFilter = new SpatialFilterClass();
                    spatialFilter.Geometry = queryGeometry;
                    spatialFilter.GeometryField = featClass.ShapeFieldName;
                    spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects; //esriSpatialRelEnum.esriSpatialRelTouches;
                    // Execute the query and iterate through the cursor's results.
                    IFeatureCursor featCursor = featClass.Search(spatialFilter, false);
                    IFeature feat = featCursor.NextFeature();
                    double length = 0.0;
                    while (feat != null)
                    {
                        ICurve curve = feat.ShapeCopy as ICurve;
                        if ( curve.Length > length)//DisplayMap.AreGeometriesTheSame(point, curve.FromPoint, false) == false
                            //&& DisplayMap.AreGeometriesTheSame(point, curve.ToPoint, false) == false
                            //&& (otherPt == null || (DisplayMap.AreGeometriesTheSame(otherPt, curve.FromPoint, false) == false
                            //&& DisplayMap.AreGeometriesTheSame(otherPt, curve.ToPoint, false) == false))
                            //&&
                        {
                            nearestFeat = feat;
                            length = curve.Length;
                        }
                        feat = featCursor.NextFeature();
                    }

                    // The cursors is no longer needed, so dispose of it.
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(featCursor);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(featClass);
                }
            }
            return nearestFeat;
        }

        protected override void OnMouseMove(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
            IPoint p =ArcMap.Document.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y);
            if (DisplayMap.IsPointInExtent(p, ArcMap.Document.ActiveView.Extent))
            {
                if (p != null)
                {
                    double pX = DatabaseUtil.ToDouble(p.X, 2);
                    double pY = DatabaseUtil.ToDouble(p.Y, 2);
                    double segmentLength = 0.0;
                    double totalLength = 0.0;
                    double arithAngle = 0.0;
                    double geoAngle = 0.0;

                    if (_insertionFeedbackInUse && _insertionFeedback != null)
                    {
                        if (_constrainedDistance != 0)
                        {
                            if (_segmentAnchorPoint != null)
                                p.ConstrainDistance(_constrainedDistance, _segmentAnchorPoint);
                            else
                                p.ConstrainDistance(_constrainedDistance, _initialAnchorPoint);
                        }

                        if (_constrainedAngle != 0)
                        {
                            if (_segmentAnchorPoint != null)
                                p.ConstrainAngle(_constrainedAngle, _segmentAnchorPoint, true);
                            else
                                p.ConstrainAngle(_constrainedAngle, _initialAnchorPoint, true);
                        }
                        _constrainedAnchor = p;

                        GISApplication.SnapPoint(p);
                        _movePointFeedback.Refresh(ArcMap.Document.ActiveView.ScreenDisplay.hDC);
                        _movePointFeedback.Start(p, p);

                        _insertionFeedback.MoveTo(p);
                        _vertexPoint = p;

                        try
                        {
                            if (_initialAnchorPoint != null)
                            {
                                IPoint internalAnchor;
                                if (_segmentAnchorPoint == null)
                                    internalAnchor = _initialAnchorPoint;
                                else
                                    internalAnchor = _segmentAnchorPoint;

                                segmentLength = DisplayMap.DistanceBetweenTwoPoints(internalAnchor.Y, p.Y, internalAnchor.X, p.X);
                                segmentLength = DatabaseUtil.ToDouble(segmentLength, 2);

                                if (_segmentCollection.SegmentCount > 0)
                                {
                                    totalLength += segmentLength;
                                    esriSegmentInfo segInfo;
                                    IEnumSegment enumSeg = _segmentCollection.EnumSegments;

                                    enumSeg.NextEx(out segInfo);
                                    totalLength += segInfo.pSegment.Length;
                                    while (!enumSeg.IsLastInPart())
                                    {
                                        enumSeg.NextEx(out segInfo);
                                        if (segInfo.pSegment != null)
                                            totalLength += segInfo.pSegment.Length;
                                    }
                                    totalLength = DatabaseUtil.ToDouble(totalLength, 2);
                                }
                                else
                                    totalLength = segmentLength;

                                arithAngle = DisplayMap.GetArithmeticAngleOfTwoPoints(internalAnchor, p);
                                arithAngle = DatabaseUtil.ToDouble(arithAngle, 2);
                                geoAngle = DisplayMap.GetGeographicAngleOfTwoPoints(internalAnchor, p);
                                geoAngle = DatabaseUtil.ToDouble(geoAngle, 2);
                            }
                        }
                        catch (Exception ex) { throw; }
                    }
                    else
                    {
                        GISApplication.SnapPoint(p);
                        _movePointFeedback.Refresh(ArcMap.Document.ActiveView.ScreenDisplay.hDC);
                        _movePointFeedback.Start(p, p);
                    }
                    if (ArcMap.Application.StatusBar != null)
                    {
                        object[] args = new object[] { pX, pY, totalLength, segmentLength, arithAngle, geoAngle };
                        if (_insertionFeedbackInUse) ArcMap.Application.StatusBar.set_Message(0, string.Format("[Total Length]: {2}  [SegmentLength]: {3}  [Arithmetic Angle]: {4}  [Geographic Angle]: {5}", args));
                        else ArcMap.Application.StatusBar.set_Message(0, string.Format("[X]: {0}  [Y]: {1} ", args));
                    }
                    else if (OnSketchUpdate != null)
                        OnSketchUpdate(pX, pY, arithAngle, geoAngle, totalLength, segmentLength);
                }
            }
        }

        protected override void OnMouseUp(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
            if (!_insertionFeedbackInUse)
            {
                if (_newFeature.Shape.GeometryType == esriGeometryType.esriGeometryPoint)
                    DisplayMap.SelectFeature(_newFeature, ArcMap.Document.FocusMap, ArcMap.Document.ActiveView, true);
                _movePointFeedback.Stop();
            }
        }

        protected override bool OnContextMenu(int x, int y)
        {
            return base.OnContextMenu(x, y);
        }

        protected override bool OnDeactivate()
        {
            return base.OnDeactivate();
        }

        protected override void OnKeyDown(ESRI.ArcGIS.Desktop.AddIns.Tool.KeyEventArgs arg)
        {
            base.OnKeyDown(arg);
        }

        protected override void OnDoubleClick()
        {
            IGeometry geom = null;

            if (_contextMenu != null)
                _contextMenu.Close();

            IPointCollection pointCollection = null;
            if (_insertionFeedback != null && _vertexPoint != null)
            {
                if (_insertionFeedback is INewLineFeedback)
                {
                    INewLineFeedback lineEditFeed = (INewLineFeedback)_insertionFeedback;
                    lineEditFeed.Stop();
                    IPolyline polyLine = (IPolyline)_segmentCollection;
                    pointCollection = (IPointCollection)polyLine;

                    if (pointCollection.PointCount < 2)
                        MessageBox.Show("You must have at least two vertices in a line.", "Bad Line Geometry", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    else
                        geom = (IGeometry)pointCollection;
                }
                else if (_insertionFeedback is INewPolygonFeedback)
                {
                    INewPolygonFeedback polyFeed = (INewPolygonFeedback)_insertionFeedback;
                    polyFeed.AddPoint(_vertexPoint);

                    IPolygon polygon = polyFeed.Stop();
                    if (polygon != null)
                        pointCollection = (IPointCollection)polygon;
                    if (pointCollection.PointCount < 3)
                        MessageBox.Show("You must have at least three vertices in a polygon.", "Bad Polygon Geometry", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    else
                        geom = (IGeometry)pointCollection;
                }
                if (geom != null)
                {
                    CreateFeature(geom);
                    DisplayMap.SelectFeature(_newFeature, ArcMap.Document.FocusMap, ArcMap.Document.ActiveView, true);
                }
                _insertionFeedback = null;
            }
            _insertionFeedbackInUse = false;
        }

        protected override void OnRefresh(int hDC)
        {
            try
            {
                if (_insertionFeedbackInUse)
                {
                    if (_insertionFeedback != null)
                        _insertionFeedback.Refresh(hDC);
                }
                if (_movePointFeedback != null)
                    _movePointFeedback.Refresh(hDC);
            }
            catch
            {
            }
        }

        protected override void OnActivate()
        {
            _moveDeviceAlongLineOnInsert = false;
            IFeatureLayer layer = ExtensionInfo.FeatureLyrByFCName.ContainsKey(Associated_FeatureClassName) ? ExtensionInfo.FeatureLyrByFCName[Associated_FeatureClassName] : null;

            if (layer != null)
            {
                _currentTargetLayer = layer as IGeoFeatureLayer;
                if (_currentTargetLayer != null)
                {
                    _movePointFeedback.Display = ArcMap.Document.ActiveView.ScreenDisplay;
                    _workspace = ExtensionInfo.Editor.EditWorkspace;
                    if (_currentTargetLayer != null)
                    {
                        if (this is IElectricPrimaryDeviceClass && this is IStepTransformerBankClass == false)
                            _moveDeviceAlongLineOnInsert = true;
                    }
                }
                else
                    DeactivateCurrentTool();
            }
        }

        private void DeactivateCurrentTool()
        {
            _newFeature = null;
            if (_movePointFeedback != null) _movePointFeedback.Stop();
            if (_insertionFeedback != null) _insertionFeedback = null;
            _insertionFeedbackInUse = false;
        }


        private void StopSketch()
        {
            if (_contextMenu != null) _contextMenu.Close();

            if (_newFeature != null || _operationStarted)
            {
                GISApplication.AbortEditOperation(ExtensionInfo.Editor.EditWorkspace);
                _operationStarted = false;

            }
            else
            {
                if (_insertionFeedback is INewLineFeedback)
                {
                    INewLineFeedback lineEditFeed = (INewLineFeedback)_insertionFeedback;
                    lineEditFeed.Stop();
                }
                else if (_insertionFeedback is INewPolygonFeedback)
                {
                    INewPolygonFeedback polyFeed = (INewPolygonFeedback)_insertionFeedback;
                    polyFeed.Stop();
                }
            }
            DisplayMap.PartialRefresh(ArcMap.Document.ActiveView);
            DeactivateCurrentTool();
        }

        private void DisplayContextMenu(esriGeometryType geometryType)
        {
            if (_contextMenu != null)
                _contextMenu.Close();
            ConstructContextMenu(geometryType);
            if (_segmentCollection != null)
            {
                if (_segmentCollection.SegmentCount > 0)
                    _contextMenu.Controls[0].Controls[0].Enabled = true;
                else
                    _contextMenu.Controls[0].Controls[0].Enabled = false;
            }
            _contextMenu.Show();
            _contextMenu.Location = System.Windows.Forms.Cursor.Position;
        }

        private void ConstructContextMenu(esriGeometryType geometryType)
        {
            _contextMenu = new Form();
            _contextMenu.TopLevel = true;
            _contextMenu.ShowInTaskbar = false;
            if (geometryType == esriGeometryType.esriGeometryPoint)
                _contextMenu.Size = new Size(50, 30);
            else
                _contextMenu.Size = new Size(75, 180);
            _contextMenu.FormBorderStyle = FormBorderStyle.None;

            _contextMenu.LostFocus += _contextMenu_LostFocus;

            FlowLayoutPanel flowPanel = new FlowLayoutPanel { Size = _contextMenu.Size, Margin = new Padding(5), FlowDirection = System.Windows.Forms.FlowDirection.TopDown };
            flowPanel.Paint += flowPanel_Paint;

            _contextMenu.Controls.Add(flowPanel);

            if (geometryType == esriGeometryType.esriGeometryPolyline || geometryType == esriGeometryType.esriGeometryPolygon)
            {
                Label finish = new Label { Text = "Finish Sketch", Margin = new Padding(1), Image = Properties.Resources.add };
                finish.Click += finish_Click;
                finish.TextAlign = ContentAlignment.MiddleLeft;
                finish.ImageAlign = ContentAlignment.MiddleRight;
                finish.MouseEnter += finish_MouseEnter;
                finish.MouseLeave += finish_MouseLeave;

                Label delete = new Label { Text = "Delete Sketch", Margin = new Padding(1), Image = Properties.Resources.delete };
                delete.Click += delete_Click;
                delete.TextAlign = ContentAlignment.MiddleLeft;
                delete.ImageAlign = ContentAlignment.MiddleRight;
                delete.MouseEnter += delete_MouseEnter;
                delete.MouseLeave += delete_MouseLeave;

                Label deleteVertex = new Label { Text = "Delete Vertex", Margin = new Padding(1), Image = Properties.Resources.delete };
                deleteVertex.Click += deleteVertex_Click;
                deleteVertex.TextAlign = ContentAlignment.MiddleLeft;
                deleteVertex.ImageAlign = ContentAlignment.MiddleRight;
                deleteVertex.MouseEnter += deleteVertex_MouseEnter;
                deleteVertex.MouseLeave += deleteVertex_MouseLeave;

                Label distAngle = new Label { Text = "Distance/Angle", Margin = new Padding(1), Image = Properties.Resources.application_form_add, TextAlign = ContentAlignment.MiddleLeft, ImageAlign = ContentAlignment.MiddleRight };
                distAngle.Click += distAngle_Click;
                distAngle.MouseEnter += distAngle_MouseEnter;
                distAngle.MouseLeave += distAngle_MouseLeave;

                Label twoPart = new Label { Text = "2-Part Polyline", Margin = new Padding(1), Image = Properties.Resources.brick, TextAlign = ContentAlignment.MiddleLeft, ImageAlign = ContentAlignment.MiddleRight };
                twoPart.Click += twoPart_Click;
                twoPart.MouseEnter += twoPart_MouseEnter;
                twoPart.MouseLeave += twoPart_MouseLeave;

                Label distanceConstrain = new Label { Text = "Distance", Margin = new Padding(1), Image = Properties.Resources.ruler_2, TextAlign = ContentAlignment.MiddleLeft, ImageAlign = ContentAlignment.MiddleRight };
                distanceConstrain.Click += distanceConstrain_Click;
                distanceConstrain.MouseEnter += distanceConstrain_MouseEnter;
                distanceConstrain.MouseLeave += distanceConstrain_MouseLeave;

                Label angleConstrain = new Label { Text = "Angle", Margin = new Padding(1), Image = Properties.Resources.ruler_triangle, TextAlign = ContentAlignment.MiddleLeft, ImageAlign = ContentAlignment.MiddleRight };
                angleConstrain.Click += angleConstrain_Click;
                angleConstrain.MouseEnter += angleConstrain_MouseEnter;
                angleConstrain.MouseLeave += angleConstrain_MouseLeave;

                flowPanel.Controls.Add(deleteVertex);
                flowPanel.Controls.Add(delete);
                flowPanel.Controls.Add(finish);
                flowPanel.Controls.Add(distanceConstrain);
                flowPanel.Controls.Add(angleConstrain);
                flowPanel.Controls.Add(distAngle);
                flowPanel.Controls.Add(twoPart);
            }
            else if (geometryType == esriGeometryType.esriGeometryPoint)
            {
                Label absXY = new Label { Text = "Absolute X,Y", Margin = new Padding(1), Image = Properties.Resources.NeedleLeftYellow2, TextAlign = ContentAlignment.MiddleLeft, ImageAlign = ContentAlignment.MiddleRight };
                absXY.Click += absXY_Click;
                absXY.MouseEnter += absXY_MouseEnter;
                absXY.MouseLeave += absXY_MouseLeave;

                flowPanel.Controls.Add(absXY);
            }
        }

        private void ProcessSegment(IPoint anchor)
        {
            ISegment segment = new LineClass();
            if (_segmentAnchorPoint == null)
                segment.FromPoint = _initialAnchorPoint;
            else
                segment.FromPoint = _segmentAnchorPoint;
            segment.ToPoint = anchor;
            _segmentAnchorPoint = anchor;

            object o = Type.Missing;
            _segmentCollection.AddSegment(segment, ref o, ref o);

            if (_insertionFeedback is INewLineFeedback)
            {
                INewLineFeedback lineEditFeedback = (INewLineFeedback)_insertionFeedback;
                lineEditFeedback.AddPoint(anchor);
            }
            else if (_insertionFeedback is INewPolygonFeedback)
            {
                INewPolygonFeedback polygonEditFeedback = (INewPolygonFeedback)_insertionFeedback;
                polygonEditFeedback.AddPoint(anchor);
            }
        }

        private void RemoveLastSegment()
        {
            if (_segmentCollection.SegmentCount > 0)
            {
                _segmentCollection.RemoveSegments(_segmentCollection.SegmentCount - 1, 1, true);
                ILine line = null;
                if (_insertionFeedback is INewLineFeedback)
                {
                    INewLineFeedback lineEditFeedback = (INewLineFeedback)_insertionFeedback;
                    lineEditFeedback.Stop();
                    if (_segmentCollection.SegmentCount > 0)
                    {
                        line = (ILine)_segmentCollection.get_Segment(0);
                        lineEditFeedback.Start(line.FromPoint);
                        lineEditFeedback.AddPoint(line.ToPoint);
                        for (int i = 1; i <= _segmentCollection.SegmentCount - 1; i++)
                        {
                            line = (ILine)_segmentCollection.get_Segment(i);
                            lineEditFeedback.AddPoint(line.ToPoint);
                            if (i == _segmentCollection.SegmentCount - 1)
                                _segmentAnchorPoint = line.ToPoint;
                        }
                    }
                }
                else if (_insertionFeedback is INewPolygonFeedback)
                {
                    INewPolygonFeedback polygonEditFeedback = (INewPolygonFeedback)_insertionFeedback;
                    polygonEditFeedback.Stop();
                    if (_segmentCollection.SegmentCount > 0)
                    {
                        line = (ILine)_segmentCollection.get_Segment(0);
                        polygonEditFeedback.Start(line.FromPoint);
                        polygonEditFeedback.AddPoint(line.ToPoint);
                        for (int i = 1; i <= _segmentCollection.SegmentCount - 1; i++)
                        {
                            line = (ILine)_segmentCollection.get_Segment(i);
                            polygonEditFeedback.AddPoint(line.ToPoint);
                            if (i == _segmentCollection.SegmentCount - 1)
                                _segmentAnchorPoint = line.ToPoint;
                        }
                    }
                }
            }
        }

       
        #region Event Handlers

        private void _contextMenu_LostFocus(object sender, EventArgs e)
        {
            if (_contextMenu != null)
                _contextMenu.Close();
        }

        void absXY_Click(object sender, EventArgs e)
        {
            System.Drawing.Point loc = _contextMenu.Location;
            if (_contextMenu != null)
                _contextMenu.Close();

            using (Form absXYForm = new Form { TopLevel = true, ShowInTaskbar = false, Size = new Size(220, 50), Text = "X / Y", FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow })
            {
                absXYForm.Disposed += absXYForm_Disposed;

                NumericTextBox xBox = new NumericTextBox { TabStop = true, TabIndex = 0 };
                xBox.TextChanged += xBox_TextChanged;

                NumericTextBox yBox = new NumericTextBox { TabStop = true, TabIndex = 1 };
                yBox.TextChanged += yBox_TextChanged;

                FlowLayoutPanel flowPanel = new FlowLayoutPanel { Margin = new Padding(5), Size = _contextMenu.Size, FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight };
                flowPanel.Controls.Add(xBox);
                flowPanel.Controls.Add(yBox);
                flowPanel.Dock = DockStyle.Fill;
                absXYForm.Controls.Add(flowPanel);
                absXYForm.ShowDialog();
                absXYForm.Location = loc;
            }
        }

        void absXY_MouseEnter(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Blue;
        }

        void absXY_MouseLeave(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Black;
        }

        void absXYForm_Disposed(object sender, EventArgs e)
        {
            IPoint anchor = new PointClass { X = _absX, Y = _absY };
            CreateFeature(anchor);
        }

        void xBox_TextChanged(object sender, EventArgs e)
        {
            _absX = ((NumericTextBox)sender).DoubleValue;
        }

        void yBox_TextChanged(object sender, EventArgs e)
        {
            _absY = ((NumericTextBox)sender).DoubleValue;
        }

         void distanceConstrain_Click(object sender, EventArgs e)
        {
            System.Drawing.Point loc = _contextMenu.Location;
            if (_contextMenu != null)
                _contextMenu.Close();
            using (Form distanceForm = new Form { TopLevel = true, ShowInTaskbar = false, Size = new Size(100, 50), Text = "Distance", FormBorderStyle = FormBorderStyle.FixedToolWindow })
            {

                NumericTextBox dist = new NumericTextBox { Text = _constrainedDistance.ToString(), TabStop = true, TabIndex = 0 };
                dist.TextChanged += dist_TextChanged;
                dist.KeyDown += dist_KeyDown;

                FlowLayoutPanel flowPanel = new FlowLayoutPanel { Margin = new Padding(5), Size = _contextMenu.Size, FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight };
                flowPanel.Controls.Add(dist);

                flowPanel.Dock = DockStyle.Fill;
                distanceForm.Controls.Add(flowPanel);
                distanceForm.ShowDialog();
                distanceForm.Location = loc;
            }
        }

        void distanceConstrain_MouseEnter(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Blue;
        }

        void distanceConstrain_MouseLeave(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Black;
        }

        void dist_TextChanged(object sender, EventArgs e)
        {
            _constrainedDistance = ((NumericTextBox)sender).DoubleValue;
        }

        public void dist_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ((Form)((NumericTextBox)sender).Parent.Parent).Dispose();
            }
        }

        void angleConstrain_Click(object sender, EventArgs e)
        {
            System.Drawing.Point loc = _contextMenu.Location;
            if (_contextMenu != null)
                _contextMenu.Close();
            using (Form angleForm = new Form { TopLevel = true, ShowInTaskbar = false, Size = new Size(100, 50), Text = "Angle", FormBorderStyle = FormBorderStyle.FixedToolWindow })
            {

                NumericTextBox conAngle = new NumericTextBox { Text = _constrainedDistance.ToString(), TabStop = true, TabIndex = 0 };
                conAngle.TextChanged += conAngle_TextChanged;
                conAngle.KeyDown += conAngle_KeyDown;

                FlowLayoutPanel flowPanel = new FlowLayoutPanel { Margin = new Padding(5), Size = _contextMenu.Size, FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight };
                flowPanel.Controls.Add(conAngle);

                flowPanel.Dock = DockStyle.Fill;
                angleForm.Controls.Add(flowPanel);
                angleForm.ShowDialog();
                angleForm.Location = loc;
            }
        }

        void angleConstrain_MouseEnter(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Blue;
        }

        void angleConstrain_MouseLeave(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Black;
        }

        void conAngle_TextChanged(object sender, EventArgs e)
        {

            double angle = ((NumericTextBox)sender).DoubleValue;
            IAngularConverter converter = new AngularConverterClass();
            esriDirectionType dt = ((IEditProperties2)ExtensionInfo.Editor).DirectionType;
            esriDirectionUnits ut = ((IEditProperties2)ExtensionInfo.Editor).DirectionUnits;
            converter.SetAngle(angle, esriDirectionType.esriDTPolar, ut);
            angle = converter.GetAngle(dt, ut);
            _constrainedAngle = DisplayMap.DegreesToRadians(angle);
            // newPoint.ConstructAngleDistance(anchor, radians, _distance);



            //  IAngularConverter converter = new AngularConverterClass();
            // converter.SetAngle(angle, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDURadians);
            //  angle = converter.GetAngle(esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDURadians);
            // _constrainedAngle = Mathmatics.RadiansToDegrees(angle);
        }

        void conAngle_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ((Form)((NumericTextBox)sender).Parent.Parent).Dispose();
            }
        }

        void distance_TextChanged(object sender, EventArgs e)
        {
            _distance = ((NumericTextBox)sender).DoubleValue;
        }

        void angle_TextChanged(object sender, EventArgs e)
        {
            _angle = ((NumericTextBox)sender).DoubleValue;
        }

        void delete_MouseEnter(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Blue;
        }

        void delete_MouseLeave(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Black;
        }

        void delete_Click(object sender, EventArgs e)
        {
            _contextMenu.Close();
            StopSketch();
        }

        void deleteVertex_Click(object sender, EventArgs e)
        {
            _contextMenu.Close();
            RemoveLastSegment();
        }

        void deleteVertex_MouseEnter(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Blue;
        }

        void deleteVertex_MouseLeave(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Black;
        }

        void distAngle_Click(object sender, EventArgs e)
        {
            _angle = 45.0;
            _distance = 10.0;

            System.Drawing.Point loc = _contextMenu.Location;
            if (_contextMenu != null)
                _contextMenu.Close();

            using (Form distanceAngle = new Form { TopLevel = true, ShowInTaskbar = false, Size = new Size(220, 50), Text = "Distance / Angle", FormBorderStyle = FormBorderStyle.FixedToolWindow })
            {
                _isDistAngleFormDisposed = false;
                distanceAngle.KeyDown += distanceAngle_KeyDown;
                distanceAngle.Disposed += distanceAngle_Disposed;

                NumericTextBox distance = new NumericTextBox { TabStop = true, TabIndex = 0 };
                distance.TextChanged += distance_TextChanged;
                distance.KeyDown += distance_KeyDown;

                NumericTextBox angle = new NumericTextBox { TabStop = true, TabIndex = 1 };
                angle.TextChanged += angle_TextChanged;
                angle.KeyDown += angle_KeyDown;

                FlowLayoutPanel flowPanel = new FlowLayoutPanel { Margin = new Padding(5), Size = _contextMenu.Size, FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight };
                flowPanel.Controls.Add(distance);
                flowPanel.Controls.Add(angle);
                flowPanel.Dock = DockStyle.Fill;
                distanceAngle.Controls.Add(flowPanel);
                distanceAngle.Location = loc;
                distanceAngle.ShowDialog();
                // distanceAngle.Location = loc;
            }
        }

        void angle_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ((Form)((NumericTextBox)sender).Parent.Parent).Dispose();
            }
        }

        void distance_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ((Form)((NumericTextBox)sender).Parent.Parent).Dispose();
            }
        }

        void distanceAngle_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ((Form)((NumericTextBox)sender).Parent.Parent).Close();
            }
        }

        void distAngle_MouseEnter(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Blue;
        }

        void distAngle_MouseLeave(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Black;
        }

        void distanceAngle_Disposed(object sender, EventArgs e)
        {
            if (_isDistAngleFormDisposed) return;

            double x = 0, y = 0;
            IPoint anchor = new PointClass();
            if (_segmentCollection.SegmentCount > 0)
            {
                try
                {
                    ISegment seg = _segmentCollection.get_Segment(_segmentCollection.SegmentCount - 1);
                    x = seg.ToPoint.X;
                    y = seg.ToPoint.Y;
                }
                catch
                {
                }
            }
            else
            {
                x = _initialAnchorPoint.X;
                y = _initialAnchorPoint.Y;
            }

            if (x > 0 && y > 0 && _distance > 10)
            {
                anchor.X = x;
                anchor.Y = y;
                IAngularConverter converter = new AngularConverterClass();
                esriDirectionType dt = ((IEditProperties2)ExtensionInfo.Editor).DirectionType;// GISApplication.EditorDirectionType();
                esriDirectionUnits ut = ((IEditProperties2)ExtensionInfo.Editor).DirectionUnits;// GISApplication.EditorDirectionUnit();
                converter.SetAngle(_angle, esriDirectionType.esriDTPolar, ut);
                _angle = converter.GetAngle(dt, ut);
                IConstructPoint newPoint = new PointClass();
                double radians = DisplayMap.DegreesToRadians(_angle);
                newPoint.ConstructAngleDistance(anchor, radians, _distance);
                ProcessSegment((IPoint)newPoint);
            }

            _isDistAngleFormDisposed = true;
        }

        void finish_MouseEnter(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Blue;
        }

        void finish_MouseLeave(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Black;
        }

        void finish_Click(object sender, EventArgs e)
        {
            _contextMenu.Close();
            OnDoubleClick();
        }

        void twoPart_MouseLeave(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Black;
        }

        void twoPart_MouseEnter(object sender, EventArgs e)
        {
            ((Control)sender).ForeColor = Color.Blue;
        }

        void twoPart_Click(object sender, EventArgs e)
        {
            if (_segmentCollection != null)
            {
                if (_segmentCollection.SegmentCount > 0)
                {
                    if (_insertionFeedback != null && _vertexPoint != null)
                    {
                        if (_insertionFeedback is INewLineFeedback)
                        {
                            INewLineFeedback lineEditFeed = (INewLineFeedback)_insertionFeedback;
                            lineEditFeed.Stop();

                            IEnumSegment enumSegs = _segmentCollection.EnumSegments;
                            enumSegs.Reset();

                            int i = 0, j = 0;
                            ISegment segment;
                            enumSegs.Next(out segment, ref i, ref i);
                            while (segment != null)
                            {
                                IPolyline polyLine = new PolylineClass { FromPoint = segment.FromPoint, ToPoint = segment.ToPoint };
                                CreateFeature(polyLine as IGeometry);
                                j++;
                                enumSegs.Next(out segment, ref i, ref i);
                            }
                        }
                        _insertionFeedback = null;
                    }
                    _insertionFeedbackInUse = false;
                }
            }
        }

        void flowPanel_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rec = ((Control)sender).ClientRectangle;
            ControlPaint.DrawBorder3D(e.Graphics, rec, Border3DStyle.Etched);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing && _contextMenu != null)
            {
                _contextMenu.Close();
                _contextMenu.Dispose();
                _contextMenu = null;
            }
        }

        private void CreateFeature(IGeometry geometry)
        {
            if (geometry == null)
                return;

            if (_currentTargetLayer == null)
                return;

            _operationStarted = false;
            bool alreadyInEditOperation = GISApplication.IsInEditOperation(_workspace);
            if (alreadyInEditOperation)
                GISApplication.StopEditOperation(_workspace);

            //SRIRAM:calc connectivity for new feature; if invalid feature, return not creating feature. We are not sure what is invalid yet.
            

            ConnectivityInfo eoConn = null; //some of the info is for nodes only and some for sections only.//TODO

            if (geometry.GeometryType == esriGeometryType.esriGeometryPoint)
            {
                IPoint pt = (IPoint)geometry;
                eoConn = ExtensionInfo.checkFlow.GetParent(pt.X, pt.Y);
            }
            if (geometry.GeometryType == esriGeometryType.esriGeometryPolyline)
            {
                IPolyline ln = (IPolyline)geometry;
                eoConn = ExtensionInfo.checkFlow.GetParent(ln.FromPoint.X, ln.FromPoint.Y, ln.ToPoint.X, ln.ToPoint.Y);
            }

            _operationStarted = GISApplication.StartEditOperation(_workspace);
            if (!_operationStarted)
                return;

            //if 2 lines below are inside of the EditOperation, storing Network Edges throws a COM Exception.
            _newFeature = _currentTargetLayer.FeatureClass.CreateFeature();
            if (_newFeature == null)
                return;

            try { _newFeature.Shape = geometry; }
            catch (Exception e)
            {
                DeactivateCurrentTool();
                string msg = e.Message;
                if (((fdoError)Marshal.GetHRForException(e)) == fdoError.FDO_E_CLOSED_POLYLINE) msg = "Closed polylines not allowed.";
                Exception newEx = new Exception(msg);
            }
            try
            {


            ExtensionInfo.InternalEdit = true;

            DatabaseUtil.SetDefaultValues(_newFeature); //set default values
            string guid = Utilities.GenerateGUID(true);
            DatabaseUtil.SetFieldValue(_newFeature, ExtensionInfo.netUtil.ntInfo.guidName, guid);

            if (ExtensionInfo.netUtil.ntInfo.networkClassIds.ContainsValue(_currentTargetLayer.FeatureClass.ObjectClassID))
            {
                EditObject eObj = new EditObject()
                {
                    ClassName = Associated_FeatureClassName,
                    CLSID = _currentTargetLayer.FeatureClass.ObjectClassID,
                    Connectivity = eoConn,
                    ElementType = (geometry.GeometryType == esriGeometryType.esriGeometryPoint) ? ElementType.Node : ElementType.Section,
                    OID = _newFeature.OID,
                    Partner = null, //Partner is only for split
                    Type = EditType.Add,
                    UID = guid,
                    shape = geometry
                };//TODO

                ExtensionInfo.EditQueue.Add(eObj);
                //inherit from parent
                int engPh = 128;
                string constPhase = "UnKnown";
                if (eoConn != null && eoConn.parentFeature != null)
                {
                    engPh = eoConn.phaseCode;
                    constPhase = Utilities.BitgatePhaseToStringPhase(eoConn.constructedPhaseCode);
                    IFeature parent = DatabaseUtil.GetReadOnlyFeature(ExtensionInfo.Editor.EditWorkspace, eoConn.parentFeature.classID, eoConn.parentFeature.oid);
                    if (parent != null)
                    {
                        InheritElectricAttributesFromParent(_newFeature, parent);
                        if(eObj.ElementType == ElementType.Section && eoConn.constructedPhaseCode != eoConn.parentFeature.phaseCode)
                        {
                            //TODO
                        }
                    }
                }

                //update PhaseCode, ConstPhase
                DatabaseUtil.SetFieldValue(_newFeature, ExtensionInfo.netUtil.ntInfo.phaseCodeField, engPh);

                DatabaseUtil.SetFieldValue(_newFeature, ExtensionInfo.netUtil.ntInfo.constructedPhaseField, constPhase);

                //check if it is source
                if (ExtensionInfo.netUtil.ntInfo.Source != null && ExtensionInfo.netUtil.ntInfo.Source.ClassID == _currentTargetLayer.FeatureClass.ObjectClassID)
                {
                    if (string.IsNullOrEmpty(ExtensionInfo.netUtil.ntInfo.Source.FieldName) ||
                        string.Compare(ExtensionInfo.netUtil.ntInfo.Source.FieldValue, DatabaseUtil.ToText(DatabaseUtil.GetFieldValue(_newFeature, ExtensionInfo.netUtil.ntInfo.Source.FieldName)), true) == 0)
                        eObj.IsSource = true;
                }

                //check if it is network protector
                if (ExtensionInfo.netUtil.ntInfo.NTProtector != null && ExtensionInfo.netUtil.ntInfo.NTProtector.ClassID == _currentTargetLayer.FeatureClass.ObjectClassID)
                {
                    if (string.IsNullOrEmpty(ExtensionInfo.netUtil.ntInfo.NTProtector.FieldName) ||
                        string.Compare(ExtensionInfo.netUtil.ntInfo.NTProtector.FieldValue, DatabaseUtil.ToText(DatabaseUtil.GetFieldValue(_newFeature, ExtensionInfo.netUtil.ntInfo.NTProtector.FieldName)), true) == 0)
                        eObj.IsNetworkProtector = true;
                }

                //check if it is YD transformer
                if (ExtensionInfo.netUtil.ntInfo.YDTransformer != null && ExtensionInfo.netUtil.ntInfo.YDTransformer.ClassID == _currentTargetLayer.FeatureClass.ObjectClassID)
                {
                    if (string.IsNullOrEmpty(ExtensionInfo.netUtil.ntInfo.YDTransformer.FieldName) ||
                        string.Compare(ExtensionInfo.netUtil.ntInfo.YDTransformer.FieldValue, DatabaseUtil.ToText(DatabaseUtil.GetFieldValue(_newFeature, ExtensionInfo.netUtil.ntInfo.YDTransformer.FieldName)), true) == 0)
                        eObj.IsYDTransformer = true;
                }
            }

            _newFeature.Store(); //this will fire OnCreate event

            ExtensionInfo.InternalEdit = false;

                //if (DataElement.DoesFieldExist(_newFeature, GISFieldNames.Structure_Guid_FieldName))
                //{
                //    ElectricDataElement.AssociateOrCreateStructure(_newFeature as IFeature, false);
                //    _newFeature.Store();
                //}

                GISApplication.StopEditOperation(_workspace);

                ArcMap.Document.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, _newFeature, ArcMap.Document.ActiveView.Extent);
            }
            catch(Exception ex)
            {
                if (_operationStarted)
                {
                    GISApplication.AbortEditOperation(_workspace);
                    _operationStarted = false;
                }
                DeactivateCurrentTool();
            }
            finally
            {
                _operationStarted = false;
            }

            if (alreadyInEditOperation)
                GISApplication.StartEditOperation(_workspace);

        }

        private static void InheritElectricAttributesFromParent(IFeature nFeature, IFeature parent)
        {
            try
            {
                if (nFeature != null && parent != null)
                {
                    object o;
                    IField matchingField;
                    bool setDefaultValue = true;
                    string fieldName, parentValue;
                    List<string> ignoreFields = Settings.DistributionFieldsIgnoredWhileInheritingAttributes;
                    IField subTypeField = ProcessSubTypeField(parent, nFeature);
                    for (int i = 0; i < parent.Fields.FieldCount; i++)
                    {
                        IField parentField = DatabaseUtil.GetField(parent.Class, i);
                        if (parentField != subTypeField)
                        {
                            fieldName = parentField.Name.ToLower();
                           // if(string.Compare(fieldName, NTInfo.))
                            matchingField = DatabaseUtil.GetField(nFeature, fieldName);
                            if (matchingField != null && !ignoreFields.Contains(fieldName))
                            {
                                bool isValidInheritance = false;
                                IDomain mySelfDomain = DatabaseUtil.GetFieldDomain(matchingField, nFeature);
                                IDomain parentDomain = DatabaseUtil.GetFieldDomain(parentField, parent);

                                if ((mySelfDomain == null && parentDomain == null) || (mySelfDomain == parentDomain))
                                    isValidInheritance = true;
                                else
                                {
                                    if (DatabaseUtil.HasSubtype(nFeature.Class) && DatabaseUtil.HasSubtype(parent.Class))
                                        if (DatabaseUtil.GetSubtypeField(nFeature.Class) == matchingField && DatabaseUtil.GetSubtypeField(parent.Class) == parentField)
                                            if (mySelfDomain != null && DatabaseUtil.DoesDomainMemberExist(mySelfDomain as ICodedValueDomain, parent.get_Value(i)))
                                                isValidInheritance = true;
                                }

                                if (isValidInheritance)
                                {
                                    o = parent.get_Value(i);
                                    parentValue = DatabaseUtil.ToText(o);
                                    //if (DatabaseUtil.AreStringsEqual(matchingField.Name, ElectricFieldNames.PhaseCode_FieldName))
                                    //{
                                    //    tempPhase = Converter.ToInteger(o, 128);
                                    //    phaseCodeSum = PhaseCode.AddPhaseCodes(phaseCodeSum, tempPhase);
                                    //}
                                    //else
                                    //{
                                        setDefaultValue = string.IsNullOrEmpty(parentValue);
                                        if (!setDefaultValue || (matchingField == DatabaseUtil.GetSubtypeField(nFeature.Class) && nFeature.Class == parent.Class))
                                            setDefaultValue = !DatabaseUtil.SetFieldValue(nFeature, fieldName, o);
                                        if (setDefaultValue)
                                            DatabaseUtil.SetDefaultValueForField(nFeature, matchingField);
                                   // }
                                }
                            }


                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private static IField ProcessSubTypeField(IFeature parent, IFeature child)
        {
            IField childSubTypeField = DatabaseUtil.GetSubTypeField(child);
            IField parentSubTypeField = DatabaseUtil.GetSubTypeField(parent);

            if (childSubTypeField != null && parentSubTypeField != null
                && string.Compare(childSubTypeField.Name, parentSubTypeField.Name, true) == 0)
            {
                object o = DatabaseUtil.GetFieldValue(parent, parentSubTypeField.Name);
                string parentValue = DatabaseUtil.ToText(o);

                bool setDefaultValue = string.IsNullOrEmpty(parentValue);

                if (!setDefaultValue)
                    setDefaultValue = !DatabaseUtil.SetFieldValue(child, childSubTypeField.Name, o);
                if (setDefaultValue)
                    DatabaseUtil.SetDefaultValueForField(child, childSubTypeField);
            }
            else
                parentSubTypeField = null;

            return parentSubTypeField;
        }
        

    }




}
