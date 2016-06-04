    using System;
    using System.Drawing;
    using System.Reflection;
    using System.Collections.Generic;

    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.Display;
    using ESRI.ArcGIS.Geometry;
    using ESRI.ArcGIS.Geodatabase;

namespace Futura.ArcGIS.NetworkExtension
{

        public static class Cartography
        {

            #region Public Methods

            /// <summary>
            /// Create a Balloon Callout element with ObjectID for given feature.
            /// </summary>
            /// <param name="feature">The feature for which to create the callout.</param>
            /// <returns>Returns an element with a callout symbol.</returns>
            public static IElement CreateBalloonCalloutForFeature(IFeature feature)
            {
                return CreateBalloonCalloutForFeature(feature, string.Empty);
            }

            /// <summary>
            /// Creates a Balloon Callout with the value from the given field name for the given feature.
            /// </summary>
            /// <param name="displayFieldName">The name of the field.</param>
            /// <param name="feature">The feature for which to create the callout.</param>
            /// <returns>Returns an element with a callout symbol.</returns>
            public static IElement CreateBalloonCalloutForFeature(string displayFieldName, IFeature feature)
            {
                string methodName = MethodBase.GetCurrentMethod().Name;
                object o = DatabaseUtil.GetFieldValue(feature, displayFieldName, true);
                string val = o != null ? o.ToString() : string.Empty;
                return CreateBalloonCalloutForFeature(feature, val);
            }

            /// <summary>
            /// Creates a Balloon Callout with the given text and background color for the given feature.
            /// </summary>
            /// <param name="feature">The feature for which to create the callout.</param>
            /// <param name="displayText">The text to display.</param>
            /// <param name="bgColor">The background color of the callout</param>
            /// <returns>Returns an element with a callout symbol.</returns>
            public static IElement CreateBalloonCalloutForFeature(IFeature feature, string displayText, string bgColor)
            {
                ITextElement element = new TextElementClass();
                if (feature != null)
                {
                    IPoint anchor = new PointClass();
                    try
                    {
                        if (feature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                            ((IPolyline)feature.ShapeCopy).QueryPoint(esriSegmentExtension.esriNoExtension, .5, true, anchor);
                        else if (feature.Shape.GeometryType == esriGeometryType.esriGeometryPoint)
                            anchor.PutCoords(feature.ShapeCopy.Envelope.XMax, feature.ShapeCopy.Envelope.YMax);
                    }
                    catch (Exception ex)
                    {
                       // _logger.LogException(ex, "There was an error getting the shape from the feature.");
                        anchor = null;
                    }
                    if (anchor != null || !anchor.IsEmpty)
                    {
                        IBalloonCallout balloon = new BalloonCalloutClass();
                        balloon.AnchorPoint = anchor;
                        balloon.LeaderTolerance = .5;
                        balloon.Style = esriBalloonCalloutStyle.esriBCSRoundedRectangle;
                        balloon.Symbol = GenerateGenericFillSymbol(bgColor, esriSimpleFillStyle.esriSFSSolid);

                        IFormattedTextSymbol txtSym = new TextSymbolClass();
                        txtSym.Background = balloon as ITextBackground;
                        txtSym.Size = 10;

                        if (string.IsNullOrEmpty(displayText))
                            displayText = feature.OID.ToString();

                        ((IElement)element).Geometry = feature.ShapeCopy;//offSet;
                        element.Text = displayText;
                        element.Symbol = txtSym;
                    }
                }
               
                return element as IElement;
            }

            /// <summary>
            /// Creates a Balloon Callout with the given text for the given feature with a random generated background color.
            /// </summary>
            /// <param name="feature">The feature for which to create a balloon callout.</param>
            /// <param name="displayText">The text to display in the balloon callout.</param>
            /// <returns>Returns an element with a callout symbol.</returns>
            public static IElement CreateBalloonCalloutForFeature(IFeature feature, string displayText)
            {
                return CreateBalloonCalloutForFeature(feature, displayText, string.Empty);
            }

            /// <summary>
            /// Creates a custom symbol for feature insertion.
            /// </summary>
            /// <returns>Returns the custom symbol.</returns>
            public static ISymbol CreateInsertionSymbol()
            {
                ISymbol symbol = null;
                string methodName = MethodBase.GetCurrentMethod().Name;
                ISimpleMarkerSymbol markerSym = new SimpleMarkerSymbol();
                try
                {
                    IRgbColor rgbColor = new RgbColorClass();
                    rgbColor.Red = Color.Cyan.R;
                    rgbColor.Green = Color.Cyan.G;
                    rgbColor.Blue = Color.Cyan.B;
                    rgbColor.Transparency = 50;

                    markerSym.Size = 7;
                    markerSym.Outline = true;
                    markerSym.Color = rgbColor;
                    markerSym.Style = esriSimpleMarkerStyle.esriSMSCircle;
                    ((ISymbol)markerSym).ROP2 = esriRasterOpCode.esriROPNotXOrPen;
                }
                catch (Exception e)
                {
                }
                finally
                {
                    symbol = markerSym as ISymbol;
                }
                return symbol;
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
                string methodName = MethodBase.GetCurrentMethod().Name;
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
                }

                return sLs;
            }

            /// <summary>
            /// Generates a simple fill symbol of random color and solid fill.
            /// </summary>
            /// <returns>Returns the simple fill symbol.</returns>
            public static ISimpleFillSymbol GenerateGenericFillSymbol()
            {
                return GenerateGenericFillSymbol(string.Empty, esriSimpleFillStyle.esriSFSSolid);
            }

            /// <summary>
            /// Generates a simple fill symbol of specific color and specific fill style.
            /// </summary>
            /// <param name="colorName">The fill color.</param>
            /// <param name="style">The fill style.</param>
            /// <returns>Returns the simple fill symbol.</returns>
            public static ISimpleFillSymbol GenerateGenericFillSymbol(string colorName, esriSimpleFillStyle style)
            {
                string methodName = MethodBase.GetCurrentMethod().Name;
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
                string methodName = MethodBase.GetCurrentMethod().Name;
                System.Threading.Thread.Sleep(10);
                Random seed = new Random(DateTime.Now.Millisecond);

                IRgbColor color = new RgbColorClass();
                if (!string.IsNullOrEmpty(colorName))
                {
                    Color winColor = ColorNameToColor(colorName);
                    color.RGB = ColorTranslator.ToOle(winColor);
                }
                else
                {
                    color.Red = seed.Next();
                    color.Blue = seed.Next();
                    color.Green = seed.Next();
                }
                return color;
            }

            public static Color ColorNameToColor(string colorName)
            {
                Color c = Color.Black;
                string methodName = MethodInfo.GetCurrentMethod().Name;

                try
                {
                    c = ColorTranslator.FromHtml(colorName);
                }
                catch
                {
                   // _logger.LogFormat("{0}: Unable to convert the provided variable with the value of {1} to a color.", methodName, colorName, LogLevel.enumLogLevelWarn);
                }
                return c;
            }
            #endregion
        }
    }