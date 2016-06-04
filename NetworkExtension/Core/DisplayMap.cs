using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Futura.ArcGIS.NetworkExtension
{
    public enum LayerType : ushort
    {
        DataLayer = 0,
        GroupLayer = 1,
        FeatureLayer = 2,
        GraphicsLayer = 3,
        GeoFeatureLayer = 4
    }

    public enum WorkspaceType
    {
        FileSystem,
        PGDB,
        FileGDB,
        SDE,
        None
    }

    public class DisplayMap
    {
        #region Public Methods

        public static double AngleOfTwoPoints(double y1, double y2, double x1, double x2)
        {
            try
            {
                double dY = y2 - y1;
                double dX = x2 - x1;
                return System.Math.Atan2(dY, dX);
            }
            catch (Exception e)
            {
                //_logger.LogException(e);
            }
            return 0.0;
        }


        public static double DistanceBetweenTwoPoints(double y1, double y2, double x1, double x2)
        {
            try
            {
                double dY = y2 - y1;
                double dX = x2 - x1;
                dY = System.Math.Pow(dY, 2);
                dX = System.Math.Pow(dX, 2);
                double dXY = dX + dY;
                return System.Math.Sqrt(dXY);
            }
            catch (Exception e)
            {
               // _logger.LogException(e);
            }
            return 0;
        }

        /// <summary>
        /// Gets the arithmetic angle from two points.
        /// </summary>
        /// <param name="p1">A point.</param>
        /// <param name="p2">A point.</param>
        /// <returns>Returns the angle.</returns>
        public static double GetArithmeticAngleOfTwoPoints(IPoint p1, IPoint p2)
        {
            double angle = 0.0;
            if (IsGeometryValid(p1) && IsGeometryValid(p2))
            {
                angle = AngleOfTwoPoints(p1.Y, p2.Y, p1.X, p2.X);
                if (angle < 0)
                    angle = 360 - (((angle * 360) * -1) / (2 * System.Math.PI));
                else
                    angle = (angle * 360) / (2 * System.Math.PI);
            }
            return angle;
        }

        public static System.Drawing.Point ConstructPointFromAngleAndDistanceOfExistingXY(double x, double y, double angle, double distance)
        {
            int x2, y2;
            x2 = DatabaseUtil.ToInteger(x + (Math.Cos(angle) * distance), 0);
            y2 = DatabaseUtil.ToInteger(y + (Math.Sin(angle) * distance), 0);
            return new System.Drawing.Point(x2, y2);
        }

        public static double DegreesToRadians(double degrees)
        {
            return degrees * 2 * System.Math.PI / 360;
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * (System.Math.PI / 180);
        }
        /// <summary>
        /// Gets the geographic angle from two points.
        /// </summary>
        /// <param name="p1">A point.</param>
        /// <param name="p2">A point.</param>
        /// <returns>Returns the angle.</returns>
        public static double GetGeographicAngleOfTwoPoints(IPoint p1, IPoint p2)
        {
            double angle = 0.0;
            if (IsGeometryValid(p1) && IsGeometryValid(p2))
            {
                angle = AngleOfTwoPoints(p1.Y, p2.Y, p1.X, p2.X);
                if (angle < 0)
                    angle = 360 - (((angle * 360) * -1) / (2 * System.Math.PI));
                else
                    angle = (angle * 360) / (2 * System.Math.PI);
            }
            IAngularConverter converter = new AngularConverterClass();
            converter.SetAngle(angle, esriDirectionType.esriDTPolar, esriDirectionUnits.esriDUDecimalDegrees);
            return converter.GetAngle(esriDirectionType.esriDTNorthAzimuth, esriDirectionUnits.esriDUDecimalDegrees);
        }
        /// <summary>
        /// Gets the dataset name objects from the workspace.
        /// </summary>
        /// <param name="workspace">The workspace from which to get the dataset name objects.</param>
        /// <param name="type">The type of datasets for which to get the name objects.</param>
        /// <returns>Returns a dictionary of dataset names by qualified name.</returns>
        public static IDictionary<string, IDatasetName> GetNetworkDSNames(IWorkspace workspace, string[] nwkDSNames, string fdsName, ref IDictionary<string, int> objClassIdByName)
        {
            IDictionary<string, IDatasetName> dnames = new Dictionary<string, IDatasetName>();
            objClassIdByName = new Dictionary<string, int>();
            try
            {
                if (workspace != null)
                {
                    IEnumDatasetName names = workspace.get_DatasetNames(esriDatasetType.esriDTFeatureDataset);
                    names.Reset();

                    IDatasetName name = names.Next();
                    try
                    {
                        while (name != null)
                        {
                            string key = DisplayMap.GetQualifiedName(name, workspace);
                            if (string.Compare(key, fdsName, true) == 0)
                            {
                                GetDatasetNames(workspace, name, esriDatasetType.esriDTAny, nwkDSNames, ref dnames, ref objClassIdByName);
                                break;
                            }
                            name = names.Next();
                        }
                    }
                    catch (Exception e)
                    {
                        // _logger.LogAndDisplayException(e);
                    }
                }
                // else
                // _logger.LogFormat("{0}: Null workspace parameter.", methodName, LogLevel.enumLogLevelWarn);
            }
            catch (Exception e)
            {
                // _logger.LogAndDisplayException(e);
            }
            return dnames;
        }

        /// <summary>
        /// Determines if the given point is in the given extent.
        /// </summary>
        /// <param name="point">A point.</param>
        /// <param name="envelope">An extent.</param>
        /// <returns>Returns true if the points is in the extent, otherwise returns false.</returns>
        public static bool IsPointInExtent(IPoint point, IEnvelope envelope)
        {
            bool retVal = false;

            if (IsGeometryValid(point) && IsGeometryValid(envelope as IGeometry))
            {
                double xmin, ymin, xmax, ymax;
                envelope.QueryCoords(out xmin, out ymin, out xmax, out ymax);
                if ((point.X > xmin) && (point.Y > ymin) && (point.X < xmax) && (point.Y < ymax))
                    retVal = true;
            }

            return retVal;
        }
        /// <summary>
        /// Gets the dataset name objects from the given dataset name.
        /// </summary>
        /// <param name="dsName">The dataset name from which to get dataset name objects.</param>
        /// <param name="type">The type of dataset name objects to get.</param>
        /// <param name="names">A dictionary of dataset name objects by qualified name.</param>
        private static void GetDatasetNames(IWorkspace workspace, IDatasetName dsName, esriDatasetType type, string[] networkClasses, ref IDictionary<string, IDatasetName> names, ref IDictionary<string, int> objClassIdByName)
        {
            try
            {
                if (dsName != null)
                {
                    if (dsName.Type == type || type == esriDatasetType.esriDTAny)
                    {
                        string key = (DisplayMap.GetQualifiedName(dsName, workspace)).ToLower();
                        if (key != string.Empty)
                        {
                            foreach (string clsName in networkClasses)
                            {
                                if (string.Compare(key, clsName, true) == 0)
                                {
                                    names[clsName] = dsName;
                                    if (dsName is IObjectClassName)
                                        objClassIdByName[clsName] = ((IObjectClassName)dsName).ObjectClassID;
                                    break;
                                }
                            }
                        }
                    }

                    IEnumDatasetName dataNames = dsName.SubsetNames;
                    if (dataNames != null)
                    {
                        dataNames.Reset();
                        IDatasetName name = dataNames.Next();

                        try
                        {
                            while (name != null)
                            {
                                GetDatasetNames(workspace, name, type, networkClasses, ref names, ref objClassIdByName);
                                name = dataNames.Next();
                            }
                        }
                        catch (Exception e)
                        {
                            // _logger.LogAndDisplayException(e);
                        }
                    }
                }
                // else
                //   _logger.LogFormat("{0}: Null dataset name parameter.", methodName, LogLevel.enumLogLevelWarn);
            }
            catch (Exception e)
            {
                // _logger.LogAndDisplayException(e);
            }
        }

        private static object OpenFeatureWorkspaceObject(IWorkspace wrkSpace, string name, esriDatasetType objType)
        {
            // string methodName = MethodInfo.GetCurrentMethod().Name;
            object o = null;

            if (wrkSpace != null && name != string.Empty)
            {
                try
                {
                    if (wrkSpace is IFeatureWorkspace && wrkSpace.Type == esriWorkspaceType.esriRemoteDatabaseWorkspace)
                    {
                        string dbName = string.Empty;
                        string ownerName = string.Empty;
                        if (wrkSpace != null && name != string.Empty)
                        {
                            // GetSdeDatabaseAndOwner(wrkSpace, out ownerName, out dbName);
                            string[] users = new string[] { "DBO", "SDE" };

                            name = (name.IndexOf('.') != -1) ? name.Substring(name.LastIndexOf('.') + 1) : name;
                            for (int i = 0; i < users.Length; i++)
                            {
                                string qualifiedName = name;
                                if (wrkSpace is ISQLSyntax)
                                    qualifiedName = ((ISQLSyntax)wrkSpace).QualifyTableName(dbName, users[i], name);

                                o = OpenObjectFromWorkspace(wrkSpace, qualifiedName, objType);
                                if (o != null)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else if (wrkSpace is IFeatureWorkspace)
                        o = OpenObjectFromWorkspace(wrkSpace, name, objType);
                    // else
                    //_logger.LogFormat("{0}: Workspace is not a Feature Workspace.", methodName, LogLevel.enumLogLevelInfo);
                }
                catch (Exception e)
                {
                    //_logger.LogException(e);
                }
            }
            // else
            // _logger.LogFormat("{0}: Null workspace or name parameter.", methodName, LogLevel.enumLogLevelWarn);

            return o;
        }

        private static object OpenObjectFromWorkspace(IWorkspace wrkSpace, string name, esriDatasetType objType)
        {
            //string methodName = MethodInfo.GetCurrentMethod().Name;
            object o = null;

            IFeatureWorkspace featWorkspace = (IFeatureWorkspace)wrkSpace;
            try
            {
                switch (objType)
                {
                    case esriDatasetType.esriDTFeatureClass:
                        o = featWorkspace.OpenFeatureClass(name);
                        break;
                    case esriDatasetType.esriDTFeatureDataset:
                        o = featWorkspace.OpenFeatureDataset(name);
                        break;
                    case esriDatasetType.esriDTRelationshipClass:
                        o = featWorkspace.OpenRelationshipClass(name);
                        break;
                    case esriDatasetType.esriDTTable:
                        o = featWorkspace.OpenTable(name);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                // _logger.Log(string.Format("Failed to open object from workspace. ObjectName:{0}; Error:{1}.", name, e.ToString()), LogLevel.enumLogLevelDebug);
            }


            return o;
        }

        /// <summary>
        /// Gets the names of fields that have changes.
        /// </summary>
        /// <param name="row">The row from which to get the changed fields.</param>
        /// <returns>Returns a list of field names that have changes.</returns>
        public static IList<string> GetNamesOfChangedFields(IRow row)
        {
            IList<string> changedFieldNames = new List<string>();
            if (row != null)
            {
                IFeature feature = row as IFeature;
                IRowChanges rowChanges = row as IRowChanges;
                IFeatureChanges featureChanges = row as IFeatureChanges;

                if (rowChanges != null)
                {
                    for (int i = 0; i < row.Fields.FieldCount; i++)
                    {
                        if (rowChanges.get_ValueChanged(i))
                        {
                            if (!changedFieldNames.Contains(row.Fields.get_Field(i).Name))
                                changedFieldNames.Add(row.Fields.get_Field(i).Name);
                        }
                    }
                }
                if (featureChanges != null)
                {
                    if (featureChanges.ShapeChanged)
                    {
                        if (AreGeometriesTheSame(feature.Shape, featureChanges.OriginalShape, false))
                        {
                            IFeatureClass featureClass = feature.Table as IFeatureClass;
                            if (featureClass != null)
                            {
                                if (changedFieldNames.Contains(featureClass.ShapeFieldName))
                                    changedFieldNames.Remove(featureClass.ShapeFieldName);
                            }
                        }
                    }
                }
            }
            return changedFieldNames;
        }
        /// <summary>
        /// Compares two geometries to determine if they are the same.
        /// </summary>
        /// <param name="geo1">A geometry.</param>
        /// <param name="geo2">A geometry.</param>
        /// <param name="checkSpatialReference">Boolean determining whether or not to check to see if the two 
        ///                                     geometries have the same spatial reference.</param>
        /// <returns>Returns true if the geometries are the same, otherwise returns false.</returns>
        public static bool AreGeometriesTheSame(IGeometry geo1, IGeometry geo2, bool checkSpatialReference)
        {
            if (IsGeometryValid(geo1) && IsGeometryValid(geo2))
            {
                if (checkSpatialReference)
                {
                    if (!DoGeometriesHaveSameSpatialReference(geo1, geo2))
                        return false;
                }
                try
                {
                    IRelationalOperator relOp = geo1 as IRelationalOperator;
                    if (relOp != null)
                        return relOp.Equals(geo2);
                }
                catch (Exception e)
                {
                    // _logger.LogAndDisplayException(e);
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if two geometries have the same spatial reference.
        /// </summary>
        /// <param name="geo1">A geometry.</param>
        /// <param name="geo2">A geometry.</param>
        /// <returns>Returns true if the geometries have the same spatial reference, otherwise returns false.</returns>
        public static bool DoGeometriesHaveSameSpatialReference(IGeometry geo1, IGeometry geo2)
        {
            bool same = false;
            ////string methodName = MethodBase.GetCurrentMethod().Name;
            //_logger.Enter(methodName);

            try
            {
                if (IsGeometryValid(geo1) && IsGeometryValid(geo2))
                {
                    ISpatialReference spat1 = geo1.SpatialReference;
                    ISpatialReference spat2 = geo2.SpatialReference;
                    if (spat1 != null || spat2 != null)
                    {
                        if (string.Compare(spat1.Name, spat2.Name, true) == 0)
                            spat1.IsPrecisionEqual(spat2, out same);
                    }
                    //// else
                    // _logger.Log("Null spatial references.", LogLevel.enumLogLevelError);
                }
            }
            catch (Exception e)
            {
                //_logger.LogException(e);
            }
            // _logger.Exit(methodName);
            return same;
        }

        public static IEnvelope GetExtentFromFeatures(IList<IFeature> features)
        {
            IEnvelope envelope = new EnvelopeClass();
            if (features != null)
            {
                IList<IGeometry> geometries = new List<IGeometry>();
                foreach (IFeature feature in features)
                    geometries.Add(feature.ShapeCopy);
                envelope = GetExtentFromGeometries(geometries);
            }
            return envelope;
        }

        /// <summary>
        /// Checks for empty geometry.
        /// </summary>
        /// <param name="geometry">A geometry.</param>
        /// <returns>Returns true if the geometry is valid, otherwise returns false.</returns>
        public static bool IsGeometryValid(IGeometry geometry)
        {
            if (geometry != null)
            {
                if (!geometry.IsEmpty)
                    return true;
            }
            return false;
        }

        public static IEnvelope GetExtentFromGeometries(IList<IGeometry> geometries)
        {
            IEnvelope envelope = new EnvelopeClass();
            if (geometries != null)
            {
                try
                {
                    bool initialized = false;
                    double xMax = 0.0, yMax = 0.0, xMin = 0.0, yMin = 0.0, xMax2 = 0.0, yMax2 = 0.0, xMin2 = 0.0, yMin2 = 0.0, oxMax = 0.0, oyMax = 0.0, oxMin = 0.0, oyMin = 0.0;
                    foreach (IGeometry geometry in geometries)
                    {
                        if (DisplayMap.IsGeometryValid(geometry))
                        {
                            xMin2 = geometry.Envelope.XMin;
                            xMax2 = geometry.Envelope.XMax;

                            yMin2 = geometry.Envelope.YMin;
                            yMax2 = geometry.Envelope.YMax;

                            if (initialized)
                            {
                                if (xMin2 < xMin) xMin = xMin2;
                                if (yMin2 < yMin) yMin = yMin2;
                                if (xMax2 > xMax) xMax = xMax2;
                                if (yMax2 > yMax) yMax = yMax2;
                            }
                            else
                            {
                                xMin = xMin2;
                                xMax = xMax2;
                                yMin = yMin2;
                                yMax = yMax2;
                                initialized = true;
                            }
                        }
                    }
                    envelope.PutCoords(xMin, yMin, xMax, yMax);
                }
                catch { throw; }
            }
            return envelope;
        }

        public static void PanToFeatures(IList<IFeature> features, IActiveView activeView)
        {
            if (features != null)
            {
                List<IGeometry> geometries = new List<IGeometry>();
                foreach (IFeature feature in features)
                    geometries.Add(feature.ShapeCopy);
                PanToGeometries(geometries, activeView);
            }
        }

        public static void PanToGeometries(IList<IGeometry> geometries, IActiveView activeView)
        {
            if (geometries != null && activeView != null)
            {
                IEnvelope envelope = GetExtentFromGeometries(geometries);
                IDisplayTransformation displayTransformation = activeView.ScreenDisplay.DisplayTransformation;
                if (DisplayMap.IsGeometryValid(envelope as IGeometry))
                {
                    if (envelope.SpatialReference == null || envelope.SpatialReference.Equals(activeView.FocusMap.SpatialReference) == false)
                        envelope.Project(activeView.FocusMap.SpatialReference);
                    displayTransformation.VisibleBounds = envelope;
                    activeView.Refresh();
                }
            }
        }

        public static void AddFeaturesToSelection(IFeatureLayer featureLayer, int[] oids)
        {
            DisplayMap.AddFeaturesToSelection(featureLayer, oids, string.Empty);
        }

        /// <summary>
        /// Get the workspace from the class.
        /// </summary>
        /// <param name="c">The class from which to get the workspace.</param>
        /// <returns>Returns a workspace.</returns>
        public static IWorkspace GetWorkspaceFromClass(IClass c)
        {

            IWorkspace wrkSpace = null;
            if (c != null)
            {
                if (c is IDataset)
                {
                    try
                    {
                        wrkSpace = ((IDataset)c).Workspace;
                    }
                    catch (Exception e)
                    {
                        //_logger.LogException(e, String.Format("Error getting the workspace from {0}", GetQualifiedName(c))); 
                    }
                }
            }
            //else
            //    _logger.LogFormat("{0}: Null class parameter.", methodName, LogLevel.enumLogLevelWarn);


            return wrkSpace;
        }

        public static void AddFeaturesToSelection(IFeatureLayer featureLayer, int[] oids, string whereClause)
        {
            if (featureLayer != null && oids != null)
            {
                try
                {
                    IFeatureClass featureClass = featureLayer.FeatureClass;
                    IFeatureSelection featureSelection = featureLayer as IFeatureSelection;
                    if (featureSelection != null && featureClass != null && oids.Length > 0)
                    {
                        IWorkspace workspace = DisplayMap.GetWorkspaceFromClass(featureClass);
                        if (workspace != null)
                        {
                            IQueryFilter2 queryFilter = new QueryFilterClass();
                            queryFilter.WhereClause = featureClass.OIDFieldName + " < 0";
                            ISelectionSet selectionSet = featureClass.Select(queryFilter, esriSelectionType.esriSelectionTypeIDSet, esriSelectionOption.esriSelectionOptionNormal, workspace);
                            selectionSet.AddList(oids.Length, ref oids[0]);
                            queryFilter = new QueryFilterClass();
                            if (string.IsNullOrEmpty(whereClause) == false)
                            {
                                queryFilter.WhereClause = whereClause;
                                selectionSet = selectionSet.Select(queryFilter, esriSelectionType.esriSelectionTypeIDSet, esriSelectionOption.esriSelectionOptionNormal, workspace);
                            }
                            featureSelection.SelectionSet = selectionSet;
                        }
                    }
                }
                catch { }
            }
        }

        public static void AddFeaturesToSelection(IFeatureLayer featureLayer, string fldName, string[] values)
        {
            if (featureLayer != null && values != null)
            {
                try
                {
                    //  IFeatureClass featureClass = featureLayer.FeatureClass;
                    IFeatureSelection featureSelection = featureLayer as IFeatureSelection;
                    if (featureSelection != null && values.Length > 0)
                    {
                        IQueryFilter qfilter = new QueryFilterClass();
                        string strGUIDs = string.Empty;
                        //construct Whereclause with OIDs
                        foreach (string guid in values)
                        {
                            if (string.IsNullOrEmpty(strGUIDs) == false)
                                strGUIDs += ",";

                            strGUIDs += string.Format("'{0}'", guid);
                        }

                        qfilter.WhereClause = string.Format("{0} IN ({1})", fldName, strGUIDs);

                        // ISelectionSet selectionSet = featureClass.Select(qfilter, esriSelectionType.esriSelectionTypeIDSet, esriSelectionOption.esriSelectionOptionNormal, workspace);

                        featureSelection.SelectFeatures(qfilter, esriSelectionResultEnum.esriSelectionResultNew, false);
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Creates an element from the given feature and adds it to the active view's graphics container.
        /// </summary>
        /// <param name="feature">The feature from which to create the element.</param>
        /// <param name="aView">The active view.</param>
        /// <returns>Returns a graphic element.</returns>
        public static IElement MarkFeature(IFeature feature, IActiveView aView)
        {
            return MarkFeature(feature, aView, string.Empty);
        }

        /// <summary>
        /// Generates a simple fill symbol of specific color and specific fill style.
        /// </summary>
        /// <param name="colorName">The fill color.</param>
        /// <param name="style">The fill style.</param>
        /// <returns>Returns the simple fill symbol.</returns>
        public static ISimpleFillSymbol GenerateGenericFillSymbol(string colorName, esriSimpleFillStyle style)
        {
            ISimpleFillSymbol sFs = new SimpleFillSymbolClass();
            try
            {
                if (string.IsNullOrEmpty(colorName))
                    sFs.Color = GenerateColor();
                else
                    sFs.Color = GenerateColor(colorName) as IColor;
                sFs.Style = style;
                ((ISymbol)sFs).ROP2 = esriRasterOpCode.esriROPNotXOrPen;
            }
            catch (Exception e)
            {
                //_logger.LogException(e, "An error occurred creating fill symbol in: " + methodName);
            }
            // _logger.Exit(methodName);
            return sFs;
        }

        /// <summary>
        /// Generate random RGB Color.
        /// </summary>
        /// <returns>Returns the random generated RGB color.</returns>
        public static IRgbColor GenerateColor()
        {
            return GenerateColor(string.Empty);
        }

        /// <summary>
        /// Generate specific RGB Color.
        /// </summary>
        /// <param name="colorName">The color to generate.</param>
        /// <returns>Returns the specified RGB color.</returns>
        public static IRgbColor GenerateColor(string colorName)
        {
            System.Threading.Thread.Sleep(10);
            Random seed = new Random(DateTime.Now.Millisecond);

            IRgbColor color = new RgbColorClass();
            //if (!string.IsNullOrEmpty(colorName))
            //{
            //    Color winColor = Converter.ColorNameToColor(colorName);
            //    color.RGB = ColorTranslator.ToOle(winColor);
            //}
            //else
            //{
            color.Red = seed.Next();
            color.Blue = seed.Next();
            color.Green = seed.Next();
            // }
            // _logger.Exit(methodName);
            return color;
        }
        /// <summary>
        /// Generates a simple line symbol of random color and width of 2.
        /// </summary>
        /// <returns>Returns the simple line symbol.</returns>
        public static ISimpleLineSymbol GenerateGenericLineSymbol()
        {
            return GenerateGenericLineSymbol(string.Empty, 0);
        }

        /// <summary>
        /// Generates a simple line symbol of specific color and width.
        /// </summary>
        /// <param name="colorName">The color of the line.</param>
        /// <param name="width">The width of the line.</param>
        /// <returns>Returns the simple line symbol.</returns>
        public static ISimpleLineSymbol GenerateGenericLineSymbol(string colorName, double width)
        {
            // string methodName = MethodBase.GetCurrentMethod().Name;
            // _logger.Enter(methodName);
            ISimpleLineSymbol sLs = new SimpleLineSymbolClass();
            try
            {
                sLs.Color = GenerateColor(colorName);
                if (width > 0)
                    sLs.Width = width;
                else
                    sLs.Width = 2;
                ((ISymbol)sLs).ROP2 = esriRasterOpCode.esriROPNotXOrPen;
            }
            catch (Exception e)
            {
                //_logger.LogException(e, "An error occurred creating line symbol in: " + methodName);
            }

            //_logger.Exit(methodName);
            return sLs;
        }
        /// <summary>
        /// Creates an element from the given feature and adds it to the active view's graphics container.
        /// </summary>
        /// <param name="feature">The feature from which to create the element.</param>
        /// <param name="aView">The active view.</param>
        /// <param name="markerColorName">The marker color.</param>
        /// <returns>Returns a graphic element.</returns>
        public static IElement MarkFeature(IFeature feature, IActiveView aView, string markerColorName)
        {
            IElement element = null;
            if (feature != null && aView != null)
            {
                IPolygon poly = new PolygonClass();
                try
                {
                    ITopologicalOperator topOp = feature.Shape as ITopologicalOperator;
                    poly.SpatialReference = feature.Shape.SpatialReference;
                    poly = topOp.Buffer(aView.Extent.Width * .01) as IPolygon;

                    if (poly.SpatialReference.Equals(aView.FocusMap.SpatialReference) == false)
                        poly.Project(aView.FocusMap.SpatialReference);

                    ISimpleFillSymbol fillSym = DisplayMap.GenerateGenericFillSymbol(markerColorName, esriSimpleFillStyle.esriSFSSolid);
                    fillSym.Outline = DisplayMap.GenerateGenericLineSymbol(markerColorName, 2);

                    element = new PolygonElementClass();
                    element.Geometry = poly;
                    ((IFillShapeElement)element).Symbol = fillSym;

                    IGraphicsContainer gCont = aView as IGraphicsContainer;
                    gCont.AddElement(element, 0);

                    aView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, topOp.Buffer(aView.Extent.Width * .02).Envelope);
                }
                catch (Exception e)
                {
                    // _logger.LogException(e);
                }
            }
            return element;
        }

        /// <summary>
        /// Flashes the given feature on the active view.
        /// </summary>
        /// <param name="feature">The feature to flash.</param>
        /// <param name="aView">The active view.</param>
        public static void FlashFeatureBuffer(IFeature feature, IActiveView aView)
        {
            FlashFeatureBuffer(feature, aView, string.Empty);
        }

        public static void FlashFeatureBuffer(IFeature feature, IActiveView aView, int interval)
        {
            FlashFeatureBuffer(feature, aView, string.Empty, interval);
        }

        /// <summary>
        /// Flashes the given feature on the active view.
        /// </summary>
        /// <param name="feature">The feature to flash.</param>
        /// <param name="aView">The active view.</param>
        /// <param name="colorName">The color of the flashing symbol.</param>
        public static void FlashFeatureBuffer(IFeature feature, IActiveView aView, string colorName)
        {
            if (feature != null && aView != null)
                FlashGeometry(feature.Shape, aView, colorName);
        }

        public static void FlashFeatureBuffer(IFeature feature, IActiveView aView, string colorName, int interval)
        {
            if (feature != null && aView != null)
                FlashGeometry(feature.Shape, aView, colorName, interval);
        }

        public static void FlashGeometry(IGeometry geometry, IActiveView activeView)
        {
            FlashGeometry(geometry, activeView, string.Empty);
        }

        public static void FlashGeometry(IGeometry geometry, IActiveView activeView, int interval)
        {
            FlashGeometry(geometry, activeView, string.Empty, interval);
        }

        public static void FlashGeometry(IGeometry geometry, IActiveView activeView, string colorName)
        {
            FlashGeometry(geometry, activeView, colorName, 100);
        }

        public static void FlashGeometry(IGeometry geometry, IActiveView activeView, string colorName, int interval)
        {
            if (activeView == null || !DisplayMap.IsGeometryValid(geometry))
                return;

            IPolygon poly = new PolygonClass();
            try
            {
                //this is to handle if data is in ProjectedCoordinate System & map is in Grographic Coordinate system
                if (geometry.SpatialReference.Equals(activeView.FocusMap.SpatialReference) == false)
                    geometry.Project(activeView.FocusMap.SpatialReference);

                ITopologicalOperator topOp = geometry as ITopologicalOperator;
                poly.SpatialReference = geometry.SpatialReference;
                poly = topOp.Buffer(activeView.Extent.Width * .01) as IPolygon;

                ISimpleFillSymbol fillSym = DisplayMap.GenerateGenericFillSymbol(colorName, esriSimpleFillStyle.esriSFSSolid);
                fillSym.Outline = DisplayMap.GenerateGenericLineSymbol();

                activeView.ScreenDisplay.StartDrawing(activeView.ScreenDisplay.hDC, (short)esriScreenCache.esriNoScreenCache);
                activeView.ScreenDisplay.SetSymbol(fillSym as ISymbol);

                activeView.ScreenDisplay.DrawPolygon(poly);
                System.Threading.Thread.Sleep(interval);
                activeView.ScreenDisplay.DrawPolygon(poly);
                //activeView.PartialRefresh((esriViewDrawPhase.esriViewGraphics | esriViewDrawPhase.esriViewGeoSelection), null, activeView.Extent.Envelope);
            }
            catch (Exception e)
            {
                //_logger.LogException(e);
            }
            finally
            {
                activeView.ScreenDisplay.FinishDrawing();
            }
        }

        public static IEnumFeature GetCurrentMapSelection(IMap map)
        {
            if (map == null)
                return null;
            else
                return map.FeatureSelection as IEnumFeature;
        }

        public static IEnumFeature GetFeaturesInActiveView(IMap map)
        {
            if (map == null)
                return null;

            IActiveView activeView = map as IActiveView;
            if (activeView == null)
                return null;

            map.SelectByShape(activeView.Extent, null, false);
            return map.FeatureSelection as IEnumFeature;
        }


        /// <summary>
        /// Gets the map layer for the given feature.
        /// </summary>
        /// <param name="feat">The feature for which to get the map layer.</param>
        /// <param name="map">The map.</param>
        /// <returns>Returns the layer.</returns>
        /// 

        public static ILayer GetLayerForFeature(IFeature feat, IMap map)
        {
            return GetLayerForFeature(feat, map, LayerType.GeoFeatureLayer);
        }

        /// <summary>
        /// Attempts to get the workspace from the row.
        /// </summary>
        /// <param name="row">The row from which to get the workspace.</param>
        /// <returns>Returns a workspace.</returns>
        public static IWorkspace GetWorkspaceFromRow(IRow row)
        {
            IWorkspace wrkSpace = null;
            if (row != null)
                wrkSpace = DisplayMap.GetWorkspaceFromClass(row.Table as IClass);
            // else
            // _logger.LogFormat("{0}: Null row parameter.", methodName, LogLevel.enumLogLevelWarn);

            return wrkSpace;
        }

        /// <summary>
        /// Gets the dataset name from the object.
        /// </summary>
        /// <param name="o">The object from which to get the qualified dataset name.</param>
        /// <returns>Returns the dataset name.</returns>
        public static string GetQualifiedName(object o)
        {
            IWorkspace workspace = null;
            return GetQualifiedName(o, workspace);
        }

        /// <summary>
        /// Gets the dataset name from the object.
        /// </summary>
        /// <param name="o">The object from which to get the qualified dataset name.</param>
        /// <returns>Returns the dataset name.</returns>
        public static string GetQualifiedName(object o, IWorkspace workspace)
        {
            string qualifiedName = string.Empty;
            // string methodName = MethodBase.GetCurrentMethod().Name;
            if (o != null)
            {
                string dbName, ownerName;
                GetQualifiedNameComponents(o, out dbName, out ownerName, out qualifiedName, workspace);
            }
            // else
            //  _logger.LogFormat("{0}: Null object parameter.", methodName, LogLevel.enumLogLevelWarn);
            return qualifiedName;
        }

        /// <summary>
        /// Gets the qualified owner name from the object.
        /// </summary>
        /// <param name="o">The object from which to get the qualified owner name.</param>
        /// <returns>Returns the owner name.</returns>
        public static string GetQualifiedOwnerName(object o)
        {
            string qualifiedOwnerName = string.Empty;
            // string methodName = MethodBase.GetCurrentMethod().Name;
            if (o != null)
            {
                string dbName, name;
                GetQualifiedNameComponents(o, out dbName, out qualifiedOwnerName, out name);
            }
            // else
            //  _logger.LogFormat("{0}: Null object parameter.", methodName, LogLevel.enumLogLevelWarn);

            return qualifiedOwnerName;
        }

        /// <summary>
        /// Gets the qualified database name from the object.
        /// </summary>
        /// <param name="o">The object from which to get the qualified database name.</param>
        /// <returns>Returns the database name.</returns>
        public static string GetQualifiedDatabaseName(object o)
        {
            string qualifiedDbName = string.Empty;
            // string methodName = MethodBase.GetCurrentMethod().Name;
            if (o != null)
            {
                string name, ownerName;
                GetQualifiedNameComponents(o, out qualifiedDbName, out ownerName, out name);
            }
            //else
            //  _logger.LogFormat("{0}: Null object parameter.", methodName, LogLevel.enumLogLevelWarn);

            return qualifiedDbName;
        }

        /// <summary>
        /// Parses the object's table name into it's qualified components (database name, owner name, dataset name).
        /// </summary>
        /// <param name="o">The object for which to parse the qualified name components.</param>
        /// <param name="dbName">The database name.</param>
        /// <param name="ownerName">The owner name.</param>
        /// <param name="datasetName">The dataset name.</param>
        public static void GetQualifiedNameComponents(object o, out string dbName, out string ownerName, out string datasetName)
        {
            IWorkspace workspace = null;
            GetQualifiedNameComponents(o, out dbName, out ownerName, out datasetName, workspace);
        }
        /// <summary>
        /// Attempts to get the workspace from the object.
        /// </summary>
        /// <param name="o">The object from which to get the workspace.</param>
        /// <param name="wrkSpace">IWorkspace object interface</param>
        /// <returns>Returns a workspace.</returns>
        public static IWorkspace GetWorkspace(object o)
        {
            // string methodName = MethodBase.GetCurrentMethod().Name;

            IWorkspace wrkSpace = null;
            try
            {
                IRelationshipClass relClass = o as IRelationshipClass;
                IEnumFeature enumFeature = o as IEnumFeature;
                IFeatureLayer featureLayer = o as IFeatureLayer;
                IWorkspace workspace = o as IWorkspace;
                IDataset dataset = o as IDataset;
                IName name = o as IName;
                IClass cls = o as IClass;
                IRow row = o as IRow;
                IMap map = o as IMap;

                if (featureLayer != null)
                    wrkSpace = GetWorkspace(featureLayer.FeatureClass);
                else if (workspace != null)
                    wrkSpace = workspace;
                else if (dataset != null)
                    wrkSpace = dataset.Workspace;
                else if (enumFeature != null)
                {
                    enumFeature.Reset();
                    row = enumFeature.Next();
                    wrkSpace = GetWorkspace(row);
                }
                else if (name != null)
                {
                    try
                    {
                        object openedObject = name.Open();
                        wrkSpace = GetWorkspace(openedObject);
                        Marshal.ReleaseComObject(openedObject);
                    }
                    catch
                    {
                        wrkSpace = null;
                    }
                }
                else if (relClass != null)
                    wrkSpace = GetWorkspaceFromClass(relClass.DestinationClass);
                else if (cls != null)
                    wrkSpace = GetWorkspaceFromClass(cls);
                else if (row != null)
                    wrkSpace = GetWorkspaceFromClass(row.Table as IClass);
                else if (map != null)
                    wrkSpace = DisplayMap.GetWorkspaceFromMap(map, true);
                else if (row != null)
                    wrkSpace = GetWorkspace(row.Table);
            }
            catch (Exception e)
            {
                // _logger.LogException(e, "Could not get workspace.");
            }

            return wrkSpace;
        }

        /// <summary>
        /// Parses the object's table name into it's qualified components (database name, owner name, dataset name).
        /// </summary>
        /// <param name="o">The object for which to parse the qualified name components.</param>
        /// <param name="dbName">The database name.</param>
        /// <param name="ownerName">The owner name.</param>
        /// <param name="datasetName">The dataset name.</param>
        public static void GetQualifiedNameComponents(object o, out string dbName, out string ownerName, out string datasetName, IWorkspace workspace)
        {
            dbName = string.Empty;
            ownerName = string.Empty;
            datasetName = string.Empty;
            // string methodName = MethodBase.GetCurrentMethod().Name;
            if (o != null)
            {
                string modelName = GetModelName(o);
                if (workspace == null)
                    workspace = GetWorkspace(o);
                if (workspace != null)
                {
                    if (workspace is ISQLSyntax)
                    {
                        ISQLSyntax sqlSyntax = (ISQLSyntax)workspace;
                        try
                        {
                            sqlSyntax.ParseTableName(modelName, out dbName, out ownerName, out datasetName);
                        }
                        catch (Exception e)
                        {
                            //_logger.LogException(e, String.Format("Error parsing table name for: {0}", modelName)); 
                        }
                    }
                    else
                        datasetName = modelName;
                }
            }
            // else
            // _logger.LogFormat("{0}: Null object parameter.", methodName, LogLevel.enumLogLevelWarn);

        }


        /// <summary>
        /// Gets the ModelInfo model name from the object.
        /// </summary>
        /// <param name="o">The object for which to get the model name.</param>
        /// <returns>Returns the model name.</returns>
        public static string GetModelName(object o)
        {
            // string methodName = MethodInfo.GetCurrentMethod().Name;

            string modelName = string.Empty;
            IDataset dataset = null;
            if (o != null)
            {
                try
                {
                    if (o is IDataset)
                    {
                        dataset = (IDataset)o;
                        modelName = dataset.Name;
                    }
                    else if (o is IName)
                    {
                        IName name = (IName)o;
                        IDatasetName dsName = name as IDatasetName;
                        if (dsName != null && !string.IsNullOrEmpty(dsName.Name))
                            modelName = dsName.Name;
                        else
                        {
                            dataset = name.Open() as IDataset;
                            modelName = dataset.Name;
                        }
                    }
                    else if (o is IRow)
                    {
                        IRow row = (IRow)o;
                        ITable table = row.Table;
                        modelName = GetModelName(table);
                    }
                    else if (o is IModelInfo)
                    {
                        IModelInfo modelInfo = (IModelInfo)o;
                        modelName = modelInfo.ModelName;
                    }
                }
                catch (Exception e)
                {
                    //_logger.LogException(e, "Error getting model name");
                }
            }
            //  else
            // _logger.LogFormat("{}: Null object parameter.", methodName, LogLevel.enumLogLevelWarn);
            return modelName;
        }


        public static ILayer GetLayerForFeature(IFeature feat, IMap map, LayerType layerType)
        {
            IWorkspace wrkSpace = DisplayMap.GetWorkspaceFromRow(feat);
            string qualifiedName = DisplayMap.GetQualifiedName(feat);
            IList<ILayer> layers = GetMapLayersByClassName(map, qualifiedName, layerType);

            if (wrkSpace != null)
            {
                foreach (ILayer layer in layers)
                {
                    if (wrkSpace == DisplayMap.GetWorkspace(layer))
                    {
                        IDisplayTable dispTbl = layer as IDisplayTable;
                        if (dispTbl != null)
                        {
                            try
                            {
                                IQueryFilter filter = new QueryFilterClass();
                                if (feat.HasOID)
                                {
                                    filter.WhereClause = dispTbl.DisplayTable.OIDFieldName + " = " + feat.OID;
                                    ICursor cur = dispTbl.SearchDisplayTable(filter, false);
                                    if (cur != null)
                                    {
                                        if (cur.NextRow() != null)
                                            return layer;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                // _logger.LogAndDisplayException(e);
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the layers from the map matching the specified  layer type.
        /// </summary>
        /// <param name="map">The map from which to get the map layer.</param>
        /// <param name="type">The type of map layer to get.</param>
        /// <param name="ignoreInValid">if true, only return valid layers.</param>
        /// <returns>Returns collection of map layers.</returns>
        public static IList<ILayer> GetMapLayersByLayerType(IMap map, LayerType type, bool ignoreInValid)
        {
            //string methodName = MethodInfo.GetCurrentMethod().Name;

            IList<ILayer> matchingLayers = new List<ILayer>();
            if (map != null)
            {

                ESRI.ArcGIS.esriSystem.UID layerId = GetLayerIdByLayerType(type);
                if (map.LayerCount > 0)
                {
                    try
                    {
                        IEnumLayer layers = map.get_Layers(layerId, true);
                        layers.Reset();
                        ILayer layer = layers.Next();
                        while (layer != null)
                        {
                            if (layer.Valid)
                                matchingLayers.Add(layer);
                            else if (!ignoreInValid)
                                matchingLayers.Add(layer);

                            layer = layers.Next();
                        }
                    }
                    catch (Exception ex)
                    {
                        //map.get_Layers(layerId, true) fails if layer type does not exist in map i.e looking for
                        //grouplayers and there are no grouplayers in map
                        //_logger.LogException(ex);
                    }
                }
            }
            // else
            // _logger.LogFormat("{0}: Null map parameter.", methodName, LogLevel.enumLogLevelWarn);

            return matchingLayers;
        }

        /// <summary>
        /// Gets layers in the map for the specified class name and layer type.
        /// </summary>
        /// <param name="map">The map from which to get the map layers.</param>
        /// <param name="className">The class for which to get map layers.</param>
        /// <param name="type">The type of map layers to get.</param>
        /// <returns>Returns a list of layers.</returns>
        public static IList<ILayer> GetMapLayersByClassName(IMap map, string className, LayerType type)
        {
            //string methodName = MethodInfo.GetCurrentMethod().Name;

            IList<ILayer> matchingLayers = new List<ILayer>();
            if (map != null && string.IsNullOrEmpty(className) == false)
            {

                UID layerId = GetLayerIdByLayerType(type);
                if (map.LayerCount > 0)
                {
                    try
                    {
                        IEnumLayer layers = map.get_Layers(layerId, true);
                        layers.Reset();

                        ILayer layer = layers.Next();

                        string name = string.Empty;
                        while (layer != null)
                        {
                            name = DisplayMap.GetQualifiedName(((IFeatureLayer)layer).FeatureClass);
                            if (string.Compare(name, className, true) == 0)
                            {
                                if (matchingLayers.Contains(layer) == false)
                                    matchingLayers.Add(layer);
                            }
                            layer = layers.Next();
                        }
                    }
                    catch (Exception ex)
                    {
                        //map.get_Layers(layerId, true) fails if layer type does not exist in map i.e looking for
                        //grouplayers and there are no grouplayers in map
                        // _logger.LogException(ex);
                    }
                }
            }
            //else
            //  _logger.LogFormat("{0}: Null map or layer name parameter.", methodName, LogLevel.enumLogLevelWarn);

            return matchingLayers;
        }

        /// <summary>
        /// Gets the first layer in the map for the specified class name and layer type.
        /// </summary>
        /// <param name="map">The map from which to get the map layer.</param>
        /// <param name="className">The class for which to get the map layer.</param>
        /// <param name="type">The type of map layer to get.</param>
        /// <returns>Returns a map layer.</returns>
        public static ILayer GetMapLayerByClassName(IMap map, string className, LayerType type)
        {
            ILayer layer = null;
            IList<ILayer> matchingLayers = GetMapLayersByClassName(map, className, type);
            if (matchingLayers.Count > 0)
                layer = matchingLayers[0];
            return layer;
        }

        /// <summary>
        /// Gets the layer from the map matching the specified map layer name and layer type.
        /// </summary>
        /// <param name="map">The map from which to get the map layer.</param>
        /// <param name="layerName">The name of the map layer to get.</param>
        /// <param name="type">The type of map layer to get.</param>
        /// <returns>Returns a map layer.</returns>
        public static ILayer GetMapLayerByLayerName(IMap map, string layerName, LayerType type)
        {
            // string methodName = MethodInfo.GetCurrentMethod().Name;

            ILayer mapLayer = null;
            if (map != null && string.IsNullOrEmpty(layerName) == false)
            {
                UID layerId = GetLayerIdByLayerType(type);
                if (map.LayerCount > 0)
                {
                    try
                    {
                        IEnumLayer layers = map.get_Layers(layerId, true);
                        layers.Reset();
                        ILayer layer = layers.Next();
                        while (layer != null)
                        {
                            if (string.Compare(layer.Name, layerName, true) == 0)
                            {
                                mapLayer = layer;
                                break;
                            }
                            layer = layers.Next();
                        }
                    }
                    catch (Exception ex)
                    {
                        //map.get_Layers(layerId, true) fails if layer type does not exist in map i.e looking for
                        //grouplayers and there are no grouplayers in map
                        // _logger.LogException(ex);
                    }
                }
            }
            // else
            // _logger.LogFormat("{0}: Null map or layer name parameter.", methodName, LogLevel.enumLogLevelWarn);

            return mapLayer;
        }

        /// <summary>
        /// Zoom to a point.
        /// </summary>
        /// <param name="activeView">The active view.</param>
        /// <param name="p">The point to which to zoom.</param>
        public static void ZoomToPoint(IActiveView activeView, IPoint p)
        {
            ZoomToPoint(activeView, p, 0);
        }

        /// <summary>
        /// Zoom to a point.
        /// </summary>
        /// <param name="activeView">The active view.</param>
        /// <param name="p">The point to which to zoom.</param>
        /// <param name="mapScale">The mapscale to apply to the map.</param>
        public static void ZoomToPoint(IActiveView activeView, IPoint p, double mapScale)
        {
            if (activeView != null && p != null)
                CenterOnEnvelope(activeView, p, mapScale);
        }

        /// <summary>
        /// Zoom to a feature.
        /// </summary>
        /// <param name="activeView">The active view.</param>
        /// <param name="feature">The feature to which to zoom.</param>
        public static void ZoomToFeature(IActiveView activeView, IFeature feature)
        {
            ZoomToFeature(activeView, feature, 0);
        }

        /// <summary>
        /// Zoom to a feature.
        /// </summary>
        /// <param name="activeView">The active view.</param>
        /// <param name="feature">The feature to which to zoom.</param>
        /// <param name="mapScale">The mapscale to apply to the map.</param>
        public static void ZoomToFeature(IActiveView activeView, IFeature feature, double mapScale)
        {
            if (activeView != null && feature != null)
                CenterOnEnvelope(activeView, feature.Shape, mapScale);
        }

        /// <summary>
        /// Zoom to a geometry.
        /// </summary>
        /// <param name="activeView">The active view.</param>
        /// <param name="geometry">The geometry to which to zoom.</param>
        public static void ZoomToGeometry(IActiveView activeView, IGeometry geometry)
        {
            ZoomToGeometry(activeView, geometry, 0);
        }

        /// <summary>
        /// Zoom to a geometry.
        /// </summary>
        /// <param name="activeView">The active view.</param>
        /// <param name="geometry">The geometry to which to zoom.</param>
        /// <param name="mapScale">The mapscale to apply to the map.</param>
        public static void ZoomToGeometry(IActiveView activeView, IGeometry geometry, double mapScale)
        {
            if (activeView != null && geometry != null)
                CenterOnEnvelope(activeView, geometry, mapScale);
        }

        /// <summary>
        /// Refreshes the active view.
        /// </summary>
        /// <param name="activeView">The active view.</param>
        public static void PartialRefresh(IActiveView activeView)
        {
            //string methodName = MethodInfo.GetCurrentMethod().Name;
            if (activeView != null)
                activeView.PartialRefresh((esriViewDrawPhase.esriViewGeography | esriViewDrawPhase.esriViewGeoSelection | esriViewDrawPhase.esriViewGraphics), null, null);
            // else
            // _logger.LogFormat("{0}: Null ActiveView parameter.", methodName, LogLevel.enumLogLevelWarn);
        }

        /// <summary>
        /// Adds a feature to the map selection.
        /// </summary>
        /// <param name="feature">The feature to add to the selection.</param>
        /// <param name="layer">The layer to which the feature belongs.</param>
        /// <param name="map">The map.</param>
        public static void AddFeatureToSelection(IFeature feature, ILayer layer, IMap map)
        {
            if (feature != null && layer != null && map != null)
                map.SelectFeature(layer, feature);
        }

        /// <summary>
        /// Removes features from the map selection.
        /// </summary>
        /// <param name="features">A list of features to remove from the selection.</param>
        /// <param name="layer">The layer to which the features belong.</param>
        public static void RemoveFeaturesFromSelection(IList<IFeature> features, ILayer layer)
        {
            if (features != null)
            {
                IFeatureSelection featureSelection = ((IFeatureLayer)layer) as IFeatureSelection;
                ISelectionSet selSet = featureSelection.SelectionSet;
                foreach (IFeature feature in features)
                {
                    RemoveFeatureFromSelection(feature, selSet);
                }
                featureSelection.SelectionChanged();
            }
        }

        /// <summary>
        /// Removes features from the map selection.
        /// </summary>
        /// <param name="features">A feature enumeration of those features to remove from the selection.</param>
        /// <param name="layer">The layer to which the features belong.</param>
        public static void RemoveFeaturesFromSelection(IEnumFeature features, ILayer layer)
        {
            if (features != null)
            {
                features.Reset();
                IFeature feature = features.Next();
                IFeatureSelection featureSelection = ((IFeatureLayer)layer) as IFeatureSelection;
                ISelectionSet selSet = featureSelection.SelectionSet;

                while (feature != null)
                {
                    RemoveFeatureFromSelection(feature, selSet);
                    feature = features.Next();
                }
                featureSelection.SelectionChanged();
            }
        }

        /// <summary>
        /// Removes the feature from a selection set.
        /// </summary>
        /// <param name="feature">The feature to remove from the selection set.</param>
        /// <param name="selSet">The selection set from which to remove the feature.</param>
        public static void RemoveFeatureFromSelection(IFeature feature, ISelectionSet selSet)
        {
            int oid = feature.OID;
            selSet.RemoveList(1, ref oid);
        }

        /// <summary>
        /// Gets the layer type guid.
        /// </summary>
        /// <param name="type">The layer type for which to get the UID</param>
        /// <returns>Returns the UID for the specified layer type.</returns>
        private static UID GetLayerIdByLayerType(LayerType type)
        {
            UID layerId = new UIDClass();
            layerId.Value = EsriConstants.DataLayerGUID;

            if (type == LayerType.DataLayer)
                layerId.Value = EsriConstants.DataLayerGUID;
            else if (type == LayerType.FeatureLayer)
                layerId.Value = EsriConstants.FeatureLayerGUID;
            else if (type == LayerType.GeoFeatureLayer)
                layerId.Value = EsriConstants.GeoFeatureLayerGUID;
            else if (type == LayerType.GraphicsLayer)
                layerId.Value = EsriConstants.GraphicsLayerGUID;
            else if (type == LayerType.GroupLayer)
                layerId.Value = EsriConstants.GroupLayerGUID;

            return layerId;
        }

        /// <summary>
        /// Centers the active view on the extent of the given geometry.
        /// </summary>
        /// <param name="activeView">The active view.</param>
        /// <param name="geom">The geometry on which to center the active view.</param>
        private static void CenterOnEnvelope(IActiveView activeView, IGeometry geom)
        {
            CenterOnEnvelope(activeView, geom, 0);
        }

        /// <summary>
        /// Centers the active view on the extent of the given geometry.
        /// </summary>
        /// <param name="activeView">The active view.</param>
        /// <param name="geom">The geometry on which to center the active view.</param>
        /// <param name="mapScale">The mapscale to apply to the map.</param>
        private static void CenterOnEnvelope(IActiveView activeView, IGeometry geom, double mapScale)
        {
            if (activeView != null && geom != null)
            {
                if (geom.IsEmpty == false)
                {
                    if (geom.SpatialReference.Equals(activeView.FocusMap.SpatialReference) == false)
                        geom.Project(activeView.FocusMap.SpatialReference);
                    IEnvelope env = new EnvelopeClass();
                    env = geom.Envelope;
                    if (env.Width == 0 || env.Height == 0)
                    {
                        IEnvelope activeViewEnvelope = activeView.Extent;
                        activeViewEnvelope.CenterAt(env.LowerLeft);
                        activeView.Extent = activeViewEnvelope;
                        if (mapScale == 0)
                            mapScale = 2000;
                    }
                    else
                        activeView.Extent = env;

                    if (mapScale > 0)
                        activeView.FocusMap.MapScale = mapScale;

                    activeView.Refresh();
                }
            }
        }

        /// <summary>
        /// Invalidates the screen cache for the feature.
        /// </summary>
        /// <param name="f">The feature for which to invalidate the screen cache.</param>
        /// <param name="view">The active view.</param>
        public static void InvalidateMap(IFeature f, IActiveView view)
        {
            if (f != null)
            {
                if (view != null)
                {
                    try
                    {
                        IList<IFeature> lst = new List<IFeature>();
                        lst.Add(f);
                        InvalidateMap(lst, view);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Invalidates the screen cache for the feature.
        /// </summary>
        /// <param name="features">A list of features for which to invalidate the screen cache.</param>
        /// <param name="view">The active view.</param>
        public static void InvalidateMap(IList<IFeature> features, IActiveView view)
        {
            if (features != null)
            {
                if (view != null)
                {
                    try
                    {
                        IInvalidArea invalidate = new InvalidAreaClass();
                        invalidate.Display = view.ScreenDisplay;
                        foreach (IFeature f in features)
                        {
                            invalidate.Add(f);
                        }
                        invalidate.Invalidate((short)esriScreenCache.esriAllScreenCaches);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Selects a feature on the map.
        /// </summary>
        /// <param name="feature">The feature to select.</param>
        /// <param name="map">The map.</param>
        /// <returns>Returns true if the feature was selected, otherwise returns false.</returns>
        public static bool SelectFeature(IFeature feature, IMap map, IActiveView activeView, bool isInternalSelection)
        {
            bool selectionSuccessful = false;

            if (feature != null && map != null)
            {
                try
                {
                    // Core.DeveloperSettings.IsInternalSelection = true;
                    ILayer layer = GetLayerForFeature(feature, map);
                    map.ClearSelection();

                    if (layer != null)
                    {
                        map.SelectFeature(layer, feature);

                        if (activeView != null)
                            activeView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, activeView.Extent);

                        InvalidateMap(feature, activeView);
                        selectionSuccessful = true;
                    }
                }
                catch (Exception e)
                {
                    //_logger.LogAndDisplayException(e);
                }
                finally
                {
                    // Core.DeveloperSettings.IsInternalSelection = false;
                }
            }
            return selectionSuccessful;
        }

        /// <summary>
        /// Selects a feature on the map.
        /// </summary>
        /// <param name="feature">The feature to select.</param>
        /// <param name="map">The map.</param>
        /// <returns>Returns true if the feature was selected, otherwise returns false.</returns>
        public static bool SelectFeatureOnMap(IFeature feature, IMap map)
        {
            bool selectionSuccessful = false;
            if (feature != null && map != null)
            {
                try
                {
                    //string qualifiedName = DataContainer.GetQualifiedName(feature);
                    ILayer layer = GetLayerForFeature(feature, map);

                    PartialRefresh((IActiveView)map);
                    map.ClearSelection();
                    if (layer != null)
                    {
                        map.SelectFeature(layer, feature);
                        PartialRefresh((IActiveView)map);
                        InvalidateMap(feature, (IActiveView)map);
                        selectionSuccessful = true;
                    }
                }
                catch (Exception e)
                {
                    //_logger.LogAndDisplayException(e);
                }
            }
            return selectionSuccessful;
        }

        /// <summary>
        /// Findout if the feature is already selected on map
        /// </summary>
        /// <param name="feature">feature</param>
        /// <param name="map">map</param>
        /// <returns>returns true if the feature is already selected</returns>
        public static bool IsFeatureSelected(IFeature feature, IMap map)
        {
            bool selected = false;
            if (feature != null && map != null)
            {
                try
                {
                    ILayer layer = GetLayerForFeature(feature, map);
                    if (layer != null)
                    {
                        IFeatureSelection featureSelection = ((IFeatureLayer)layer) as IFeatureSelection;
                        ISelectionSet selSet = featureSelection.SelectionSet;
                        IEnumIDs enumIds = selSet.IDs;
                        int oid = enumIds.Next();
                        while (oid != -1)
                        {
                            if (oid == feature.OID)
                                return true;
                            oid = enumIds.Next();
                        }
                    }
                }
                catch (Exception e)
                {
                    //_logger.LogAndDisplayException(e);
                }
            }
            return selected;
        }
        /// <summary>
        /// Selects a feature on the map.
        /// </summary>
        /// <param name="feature">The feature to select.</param>
        /// <param name="map">The map.</param>
        /// <returns>Returns true if the feature was selected, otherwise returns false.</returns>
        public static bool SelectFeaturesOnMap(IList<IFeature> features, IMap map)
        {
            bool selectionSuccessful = false;

            if (features != null && map != null)
            {
                IActiveView activeView = map as IActiveView;

                if (activeView != null)
                {
                    try
                    {
                        activeView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                        map.ClearSelection();

                        foreach (IFeature feature in features)
                        {
                            string qualifiedName = DisplayMap.GetQualifiedName(feature);
                            ILayer layer = GetLayerForFeature(feature, map);
                            if (layer != null)
                                map.SelectFeature(layer, feature);
                        }

                        activeView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                        InvalidateMap(features, (IActiveView)map);
                        selectionSuccessful = true;
                    }
                    catch (Exception e)
                    {
                        //_logger.LogAndDisplayException(e);
                    }
                }
            }
            return selectionSuccessful;
        }

        /// <summary>
        /// Gets the workspace being edited from the map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>Returns a workspace. </returns>
        public static IWorkspace GetWorkspaceBeingEditedFromMap(IMap map)
        {
            IWorkspace wrkSpace = null;
            IDictionary<IWorkspace, int> wrkSpaces = GetWorkspacesFromMap(map);
            foreach (KeyValuePair<IWorkspace, int> kvp in wrkSpaces)
            {
                if (kvp.Key != null)
                {
                    if (GISApplication.IsBeingEdited(kvp.Key))
                    {
                        wrkSpace = kvp.Key;
                        break;
                    }
                }
            }
            return wrkSpace;
        }
        /// <summary>
        /// Checks to see if the specified dataset exists in the workspace by name.
        /// </summary>
        /// <param name="wrkSpace">The workspace.</param>
        /// <param name="name">The name of the dataset.</param>
        /// <param name="type">The dataset type.</param>
        /// <returns>Returns true if the dataset exists in the workspace, false if it doesn't.</returns>
        public static bool DoesNameExist(IWorkspace wrkSpace, string name, esriDatasetType type)
        {
            bool nameExists = false;
            if (wrkSpace != null && name != string.Empty)
            {
                if (wrkSpace is IWorkspace2)
                    nameExists = ((IWorkspace2)wrkSpace).get_NameExists(type, name);
            }

            return nameExists;
        }

        /// <summary>
        /// Checks to see if the specified table exists in the workspace by name.
        /// </summary>
        /// <param name="wrkSpace">The workspace.</param>
        /// <param name="name">The name of the table.</param>
        /// <returns>Returns true if the table exists in the workspace, false if it doesn't.</returns>
        public static bool DoesNameExist(IWorkspace wrkSpace, string name)
        {
            return DoesNameExist(wrkSpace, name, esriDatasetType.esriDTTable);
        }

        /// <summary>
        /// Gets all the datasets from the given dataset of a specified dataset type.
        /// </summary>
        /// <param name="dSet">The dataset object from which to get the datasets.</param>
        /// <param name="type">The type of dataset to get.</param>
        /// <param name="dSets">A list of datasets.</param>
        public static void GetDatasetsByType(IDataset dSet, esriDatasetType type, ref IList<IDataset> dSets)
        {
            if (dSet != null)
            {
                if (dSet.Type == type || type == esriDatasetType.esriDTAny)
                {
                    if (!dSets.Contains(dSet))
                        dSets.Add(dSet);
                }
                IEnumDataset dataSets;
                try
                {
                    dataSets = dSet.Subsets;
                }
                catch
                {
                    dataSets = null;
                }
                if (dataSets != null)
                {
                    dataSets.Reset();
                    IDataset ds = null;
                    try { ds = dataSets.Next(); }
                    catch { ds = null; }
                    while (ds != null)
                    {
                        GetDatasetsByType(ds, type, ref dSets);
                        try { ds = dataSets.Next(); }
                        catch (Exception e)
                        {
                            ds = null;
                        }
                    }
                }
            }
        }

        public static WorkspaceType GetDatabaseType(IWorkspace wrkspace)
        {
            WorkspaceType type = WorkspaceType.None;
            if (wrkspace.Type == esriWorkspaceType.esriRemoteDatabaseWorkspace)
                type = WorkspaceType.SDE;
            else if (wrkspace.Type == esriWorkspaceType.esriLocalDatabaseWorkspace)
            {
                string wrkPath = wrkspace.PathName;
                if (string.IsNullOrEmpty(wrkPath) == false)
                {
                    if (string.Compare(System.IO.Path.GetExtension(wrkPath), ".gdb", true) == 0)
                        type = WorkspaceType.FileGDB;
                    else if (string.Compare(System.IO.Path.GetExtension(wrkPath), ".mdb", true) == 0)
                        type = WorkspaceType.PGDB;
                }
            }
            else if (wrkspace.Type == esriWorkspaceType.esriFileSystemWorkspace)
                type = WorkspaceType.FileSystem;

            return type;
        }

        public static WorkspaceType GetDatabaseType(object o)
        {

            WorkspaceType type = WorkspaceType.None;
            IWorkspace wrkspace = DisplayMap.GetWorkspace(o);
            if (wrkspace != null)
                type = GetDatabaseType(wrkspace);

            return type;
        }
        public static IGeometricNetwork GetGeometricNetworkFromWorkspace(IWorkspace workspace)
        {
            IGeometricNetwork geoNetwork = null;
            IList<IDataset> datasets = new List<IDataset>();
            GetDatasetsByType(workspace as IDataset, esriDatasetType.esriDTGeometricNetwork, ref datasets);
            foreach (IDataset dataset in datasets)
            {
                if (dataset is IGeometricNetwork)
                {
                    geoNetwork = dataset as IGeometricNetwork;
                    break;
                }
            }
            return geoNetwork;
        }

        /// <summary>
        /// Gets the workspace from the map with the most layers loaded for it.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>Returns a workspace.</returns>
        public static IWorkspace GetWorkspaceFromMap(IMap map, bool force)
        {
            IWorkspace finalWorkspace = null;
            IGeometricNetwork geoNetwork = null;
            if (map != null)
            {
                //get all workspaces from map
                IDictionary<IWorkspace, int> workspaces = GetWorkspacesFromMap(map);
                if (workspaces == null || workspaces.Count == 0) return null;

                IList<IWorkspace> validWorkspaces = new List<IWorkspace>();
                IList<IWorkspace> workspaceWithGeometricNetwork = new List<IWorkspace>();
                IEnumerator<IWorkspace> workspaceKeys = workspaces.Keys.GetEnumerator();
                workspaceKeys.Reset();
                int wsCount = 0;
                //get list of workspaces having FuturaSettings table
                while (workspaceKeys.MoveNext())
                {
                    wsCount++;
                    finalWorkspace = workspaceKeys.Current;
                    //check if workspace has FuturaSettings 
                    if (DisplayMap.DoesNameExist(finalWorkspace, "Futura_Settings", esriDatasetType.esriDTTable)
                        && validWorkspaces.Contains(finalWorkspace) == false)
                        validWorkspaces.Add(finalWorkspace);
                }

                if (wsCount > 1)
                {
                    //check if more than one database has Futura Settings
                    if (validWorkspaces.Count == 0)
                    {
                        //if no workpsace has futura settings, find out if any workspace has geometric network
                        workspaceKeys.Reset();
                        while (workspaceKeys.MoveNext())
                        {
                            geoNetwork = DisplayMap.GetGeometricNetworkFromWorkspace(workspaceKeys.Current);
                            if (geoNetwork != null && workspaceWithGeometricNetwork.Contains(workspaceKeys.Current) == false)
                                workspaceWithGeometricNetwork.Add(workspaceKeys.Current);
                            //return workspaceKeys.Current;
                        }
                    }
                    else if (validWorkspaces.Count == 1)//if only one workspace has futura settings, return it
                        return validWorkspaces[0];
                    else  //if more than one workspace has futura settings, find out which one has geometric network
                    {
                        foreach (IWorkspace ws in validWorkspaces)
                        {
                            geoNetwork = DisplayMap.GetGeometricNetworkFromWorkspace(ws);
                            if (geoNetwork != null && workspaceWithGeometricNetwork.Contains(ws) == false)
                                workspaceWithGeometricNetwork.Add(ws);
                            //return ws;
                        }
                    }

                    if (workspaceWithGeometricNetwork.Count == 1)
                        return workspaceWithGeometricNetwork[0];
                    else if (workspaceWithGeometricNetwork.Count > 1)
                        validWorkspaces = workspaceWithGeometricNetwork;
                    //get workspace from max number of layers on map, if no workspace has FuturaSettings table or Geometric network
                    IWorkspace workspace;
                    workspaceKeys.Reset();
                    int count = 0;
                    while (workspaceKeys.MoveNext())
                    {
                        workspace = workspaceKeys.Current;
                        if ((validWorkspaces.Count == 0 || validWorkspaces.Contains(workspace))
                            && workspaces[workspace] > count)
                        {
                            count = workspaces[workspace];
                            finalWorkspace = workspace;
                        }
                    }
                }

            }
            return finalWorkspace;
        }

        public static IList<IGeometricNetwork> GetGeometricNetworksFromWorkspace(IWorkspace workspace)
        {
            IFeatureDataset featureDataset;
            IList<IDataset> datasets = new List<IDataset>();
            IList<IDataset> nestedDatasets = new List<IDataset>();
            List<IGeometricNetwork> geoNetworks = new List<IGeometricNetwork>();

            GetDatasetsByType(workspace as IDataset, esriDatasetType.esriDTGeometricNetwork, ref datasets);
            foreach (IDataset dataset in datasets)
                geoNetworks.Add(dataset as IGeometricNetwork);

            datasets.Clear();
            IGeometricNetwork geoNetwork;
            GetDatasetsByType(workspace as IDataset, esriDatasetType.esriDTFeatureDataset, ref datasets);
            foreach (IDataset dataset in datasets)
            {
                featureDataset = dataset as IFeatureDataset;
                if (featureDataset != null)
                {
                    GetDatasetsByType(featureDataset as IDataset, esriDatasetType.esriDTGeometricNetwork, ref nestedDatasets);
                    foreach (IDataset nestedDataset in nestedDatasets)
                    {
                        geoNetwork = nestedDataset as IGeometricNetwork;
                        if (geoNetwork != null && !geoNetworks.Contains(geoNetwork))
                            geoNetworks.Add(geoNetwork);
                    }
                    nestedDatasets.Clear();
                }
            }
            return geoNetworks.AsReadOnly();
        }


        /// <summary>
        /// Get the workspace from map which has geometric network
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static IWorkspace GetGeoNetworkWorkspaceFromMap(IMap map, out IGeometricNetwork firstGeoNetwork)
        {
            IWorkspace workspace = null;
            firstGeoNetwork = null;
            try
            {
                IDictionary<IWorkspace, int> workspaces = DisplayMap.GetWorkspacesFromMap(map);
                if (workspaces == null) return null;

                foreach (KeyValuePair<IWorkspace, int> ws in workspaces)
                {
                    IList<IGeometricNetwork> geoNetworks = DisplayMap.GetGeometricNetworksFromWorkspace(ws.Key);
                    if (geoNetworks != null && geoNetworks.Count > 0)
                    {
                        firstGeoNetwork = geoNetworks[0];
                        workspace = ws.Key;
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                //_logger.LogException(ex);
            }
            return workspace;
        }

        /// <summary>
        /// Gets all workspaces from the map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>Returns a dictionary of the number of layers by workspace.</returns>
        public static IDictionary<IWorkspace, int> GetWorkspacesFromMap(IMap map)
        {
            IDictionary<IWorkspace, int> workspaces = new Dictionary<IWorkspace, int>();
            if (map != null && map.LayerCount > 0)
            {
                UID uid = new UIDClass();
                uid.Value = EsriConstants.FeatureLayerGUID;

                try
                {
                    IEnumLayer enumLayer = map.get_Layers(uid, true);
                    IFeatureLayer featLayer = enumLayer.Next() as IFeatureLayer;

                    IDataset ds;
                    int count = 0;
                    IWorkspace workspace;

                    while (featLayer != null)
                    {
                        try
                        {
                            if (featLayer.FeatureClass != null)
                            {
                                ds = (IDataset)featLayer.FeatureClass;
                                if (ds != null)
                                {
                                    if (ds.Workspace != null)
                                    {
                                        workspace = ds.Workspace;
                                        if (workspaces.ContainsKey(workspace))
                                        {
                                            count = workspaces[workspace];
                                            count++;
                                        }
                                        else
                                            count = 1;
                                        workspaces[workspace] = count;
                                    }
                                }
                            }
                            featLayer = enumLayer.Next() as IFeatureLayer;
                        }
                        catch (Exception e)
                        {
                            // _logger.LogException(e);
                            continue;
                        }

                    }
                }
                catch (Exception e)
                {
                    //_logger.LogException(e);
                }
            }
            return workspaces;
        }

        /// <summary>
        /// Update label expressions and definition queries when switching between databases
        /// </summary>
        /// <param name="map"></param>
        /// <param name="newWorkspace"></param>
        public static void UpdateSQLForLayer(IFeatureLayer featLayer, WorkspaceType wsType)
        {
            //string methodName = MethodInfo.GetCurrentMethod().Name;
            try
            {
                if (featLayer != null && wsType != WorkspaceType.None)
                {
                    switch (wsType)
                    {
                        case WorkspaceType.FileGDB:
                            if (featLayer is IFeatureLayerDefinition)
                            {
                                IFeatureLayerDefinition flDefinition = featLayer as IFeatureLayerDefinition;
                                if (flDefinition != null && string.IsNullOrEmpty(flDefinition.DefinitionExpression) == false)
                                {
                                    flDefinition.DefinitionExpression = flDefinition.DefinitionExpression.Replace("[", "\"");
                                    flDefinition.DefinitionExpression = flDefinition.DefinitionExpression.Replace("]", "\"");
                                    flDefinition.DefinitionExpression = flDefinition.DefinitionExpression.Replace("*", "%");
                                    flDefinition.DefinitionExpression = flDefinition.DefinitionExpression.Replace("Shape.STLength()", "Shape_Length");
                                }
                            }
                            if (featLayer is IGeoFeatureLayer)
                            {
                                IGeoFeatureLayer geoFeatLyr = featLayer as IGeoFeatureLayer;
                                if (geoFeatLyr != null)
                                {
                                    IAnnotateLayerPropertiesCollection annotateLayerPropsColl = geoFeatLyr.AnnotationProperties;
                                    if (annotateLayerPropsColl != null && annotateLayerPropsColl.Count > 0)
                                    {
                                        for (int i = 0; i < annotateLayerPropsColl.Count; i++)
                                        {
                                            IAnnotateLayerProperties annotateLayerPropetries; IElementCollection placedElements; IElementCollection unplacedElements;
                                            annotateLayerPropsColl.QueryItem(i, out annotateLayerPropetries, out placedElements, out unplacedElements);
                                            if (annotateLayerPropetries != null)
                                            {
                                                ILabelEngineLayerProperties lblEngineLyrProp = null;
                                                if (string.IsNullOrEmpty(annotateLayerPropetries.WhereClause) == false)
                                                {
                                                    annotateLayerPropetries.WhereClause = annotateLayerPropetries.WhereClause.Replace("[", "\"");
                                                    annotateLayerPropetries.WhereClause = annotateLayerPropetries.WhereClause.Replace("]", "\"");
                                                    annotateLayerPropetries.WhereClause = annotateLayerPropetries.WhereClause.Replace("*", "%");
                                                    annotateLayerPropetries.WhereClause = annotateLayerPropetries.WhereClause.Replace("Shape.STLength()", "Shape_Length");
                                                }
                                                if (annotateLayerPropetries is ILabelEngineLayerProperties)
                                                {
                                                    lblEngineLyrProp = annotateLayerPropetries as ILabelEngineLayerProperties;
                                                    if (lblEngineLyrProp != null && string.IsNullOrEmpty(lblEngineLyrProp.Expression) == false)
                                                        lblEngineLyrProp.Expression = lblEngineLyrProp.Expression.Replace("Shape.STLength()", "Shape_Length");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case WorkspaceType.SDE:
                            if (featLayer is IFeatureLayerDefinition)
                            {
                                IFeatureLayerDefinition flDefinition = featLayer as IFeatureLayerDefinition;
                                if (flDefinition != null && string.IsNullOrEmpty(flDefinition.DefinitionExpression) == false)
                                {
                                    if (flDefinition.DefinitionExpression.IndexOf("\"Shape_Length\"") != -1)
                                        flDefinition.DefinitionExpression = flDefinition.DefinitionExpression.Replace("\"Shape_Length\"", "Shape.STLength()");
                                    else if (flDefinition.DefinitionExpression.IndexOf("Shape_Length") != -1)
                                        flDefinition.DefinitionExpression = flDefinition.DefinitionExpression.Replace("Shape_Length", "Shape.STLength()");
                                }
                            }
                            if (featLayer is IGeoFeatureLayer)
                            {
                                IGeoFeatureLayer geoFeatLyr = featLayer as IGeoFeatureLayer;
                                if (geoFeatLyr != null)
                                {
                                    IAnnotateLayerPropertiesCollection annotateLayerPropsColl = geoFeatLyr.AnnotationProperties;
                                    if (annotateLayerPropsColl != null && annotateLayerPropsColl.Count > 0)
                                    {
                                        for (int i = 0; i < annotateLayerPropsColl.Count; i++)
                                        {
                                            IAnnotateLayerProperties annotateLayerPropetries; IElementCollection placedElements; IElementCollection unplacedElements;
                                            annotateLayerPropsColl.QueryItem(i, out annotateLayerPropetries, out placedElements, out unplacedElements);
                                            if (annotateLayerPropetries != null)
                                            {
                                                ILabelEngineLayerProperties lblEngineLyrProp = null;
                                                if (string.IsNullOrEmpty(annotateLayerPropetries.WhereClause) == false)
                                                {
                                                    if (annotateLayerPropetries.WhereClause.IndexOf("\"Shape_Length\"") != -1)
                                                        annotateLayerPropetries.WhereClause = annotateLayerPropetries.WhereClause.Replace("\"Shape_Length\"", "Shape.STLength()");
                                                    else if (annotateLayerPropetries.WhereClause.IndexOf("Shape_Length") != -1)
                                                        annotateLayerPropetries.WhereClause = annotateLayerPropetries.WhereClause.Replace("Shape_Length", "Shape.STLength()");
                                                }
                                                if (annotateLayerPropetries is ILabelEngineLayerProperties)
                                                {
                                                    lblEngineLyrProp = annotateLayerPropetries as ILabelEngineLayerProperties;
                                                    if (lblEngineLyrProp != null)
                                                    {
                                                        if (lblEngineLyrProp.Expression.IndexOf("Shape_Length") != -1)
                                                            lblEngineLyrProp.Expression = lblEngineLyrProp.Expression.Replace("Shape_Length", "Shape.STLength()");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
                // else
                //  _logger.LogFormat("{0}: Null FeatureLayer or workspace type is not supported.", methodName, LogLevel.enumLogLevelWarn);
            }
            catch (Exception ex)
            {
                //_logger.LogException(ex);
            }
        }

        /// <summary>
        ///Update label expressions and definition queries when switching between databases
        /// </summary>
        /// <param name="table"></param>
        /// <param name="wsType"></param>
        public static void UpdateSQLForTable(IStandaloneTable table, WorkspaceType wsType)
        {
            // string methodName = MethodInfo.GetCurrentMethod().Name;
            try
            {
                if (table != null && wsType != WorkspaceType.None)
                {
                    switch (wsType)
                    {
                        case WorkspaceType.FileGDB:
                            if (table is ITableDefinition)
                            {
                                ITableDefinition tableDefinition = table as ITableDefinition;
                                if (tableDefinition != null && string.IsNullOrEmpty(tableDefinition.DefinitionExpression) == false)
                                {
                                    tableDefinition.DefinitionExpression = tableDefinition.DefinitionExpression.Replace("[", "\"");
                                    tableDefinition.DefinitionExpression = tableDefinition.DefinitionExpression.Replace("]", "\"");
                                }
                            }
                            break;
                    }
                }
                // else
                //  _logger.LogFormat("{0}: Null Table or workspace type is not supported.", methodName, LogLevel.enumLogLevelWarn);
            }
            catch (Exception ex)
            {
                //_logger.LogException(ex);
            }
        }

        ///<summary>Convert the display extents in Pixels (at the current map scale) and then return out the map units.</summary>
        ///
        ///<param name="activeView">An IActiveView interface</param>
        ///<param name="pixelUnits">A System.Double containing the number of display pixels to convert. Example: 100</param>
        /// 
        ///<returns>A System.Double containing the number of Map Units, -1 is returned if something went wrong.</returns>
        /// 
        ///<remarks></remarks>
        public static double ConvertPixelsToMapUnits(ESRI.ArcGIS.Carto.IActiveView activeView, int pixelUnits)
        {

            if (activeView == null)
            {
                return -1;
            }

            //Get the ScreenDisplay
            ESRI.ArcGIS.Display.IScreenDisplay screenDisplay = activeView.ScreenDisplay;

            //Get the DisplayTransformation 
            ESRI.ArcGIS.Display.IDisplayTransformation displayTransformation = screenDisplay.DisplayTransformation;

            //Get the device frame which will give us the number of pixels in the X direction
            ESRI.ArcGIS.esriSystem.tagRECT deviceRECT = displayTransformation.get_DeviceFrame();
            System.Int32 pixelExtent = (deviceRECT.right - deviceRECT.left);

            //Get the map extent of the currently visible area
            ESRI.ArcGIS.Geometry.IEnvelope envelope = displayTransformation.VisibleBounds;
            System.Double realWorldDisplayExtent = envelope.Width;

            //Calculate the size of one pixel
            if (pixelExtent == 0)
            {
                return -1;
            }
            System.Double sizeOfOnePixel = (realWorldDisplayExtent / pixelExtent);

            //Multiply this by the input argument to get the result
            return (pixelUnits * sizeOfOnePixel);
        }

        #endregion

    }

    public static class EsriConstants
    {
        #region ProgIDs

        /// <summary>
        /// The prog id for ESRI Publisher Extension.
        /// </summary>
        public static string PublisherExtensionProgID
        {
            get { return "esriPublisherUI.Publisher"; }
        }

        /// <summary>
        /// The prog id for ESRI Editor Attribute Window.
        /// </summary>
        public static string AttributeWindowProgID
        {
            get { return "esriEditor.AttributeWindow"; }
        }

        /// <summary>
        /// The prog id for ESRI Editor.
        /// </summary>
        public static string ArcEditorProgID
        {
            get { return "esriEditor.Editor"; }
        }

        /// <summary>
        /// The prog id for ESRI AppRef.
        /// </summary>
        public static string ApprefProgID
        {
            get { return "esriFramework.AppRef"; }
        }

        /// <summary>
        /// The prog id for ESRI Utility Network Analysis Extension.
        /// </summary>
        public static string UtilityNetworkAnalysisExtProgID
        {
            get { return "esriEdtiorExt.UtilityNetworkAnalysisExt"; }
        }

        /// <summary>
        /// The prog id for ESRI ArcMap Snapping Window.
        /// </summary>
        public static string ArcMapSnappingWindowProgID
        {
            get { return "esriEditor.SnappingWindow"; }
        }

        #endregion

        #region GUIDs

        /// <summary>
        /// ESRI ArcEngine Target Layer Control GUID.
        /// </summary>
        public static string EngineTargetLayerControlGuid
        {
            get { return "{5D815B27-6A93-42DB-B2C6-1CC58B416E9F}"; }
        }

        /// <summary>
        /// ESRI GeoFeatureLayer GUID.
        /// </summary>
        public static string GeoFeatureLayerGUID
        {
            get { return "{E156D7E5-22AF-11D3-9F99-00C04F6BC78E}"; }
        }

        /// <summary>
        /// ESRI Feature Layer GUID.
        /// </summary>
        public static string FeatureLayerGUID
        {
            get { return "{40A9E885-5533-11d0-98BE-00805F7CED21}"; }
        }

        /// <summary>
        /// ESRI GroupLayer GUID.
        /// </summary>
        public static string GroupLayerGUID
        {
            get { return "{EDAD6644-1810-11D1-86AE-0000F8751720}"; }
        }

        /// <summary>
        /// ESRI DataLayer GUID.
        /// </summary>
        public static string DataLayerGUID
        {
            get { return "{6CA416B1-E160-11D2-9F4E-00C04F6BC78E}"; }
        }

        /// <summary>
        /// ESRI GraphicsLayer GUID.
        /// </summary>
        public static string GraphicsLayerGUID
        {
            get { return "{34B2EF81-F4AC-11D1-A245-080009B6F22B}"; }
        }

        /// <summary>
        /// ESRI SketchTool GUID.
        /// </summary>
        public static string SketchToolGUID
        {
            get { return "{B479F48A-199D-11D1-9646-0000F8037368}"; }
        }

        /// <summary>
        /// ESRI ArcEngine SketchTool GUID.
        /// </summary>
        public static string EngineSketchToolGUID
        {
            get { return "{13B234E8-3B30-49CA-9967-4C76F7231AD6}"; }
        }

        /// <summary>
        /// ESRI AppRef GUID.
        /// </summary>
        public static string ApprefGUID
        {
            get { return "{e1740ec5-9513-11d2-a2df-0000f8774fb5}"; }
        }

        /// <summary>
        /// ESRI Publisher Extension GUID.
        /// </summary>
        public static string PublisherExtensionGUID
        {
            get { return "{8AEE0DE1-535C-4788-95C8-1659444D4C02}"; }
        }

        #endregion

        #region Misc

        /// <summary>
        /// Create New Feature Task Text
        /// </summary>
        public static string CreateNewFeatureTaskText
        {
            get { return "create new feature"; }
        }

        #endregion
    }

}
