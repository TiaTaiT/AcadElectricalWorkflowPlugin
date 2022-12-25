using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using CommonHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Exception = System.Exception;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace AutocadCommands.Services
{
    public static class LinkerHelper
    {

        public static ObjectId[] SelectAllPolylineByLayer(Editor editor, string layer)
        {
            ObjectId[] retVal = null;

            try
            {
                // Get a selection set of all possible polyline entities on the requested layer
                PromptSelectionResult oPSR = null;

                TypedValue[] tvs = new TypedValue[] {
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "<and"),
                    new TypedValue(Convert.ToInt32(DxfCode.LayerName), layer),
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "<or"),
                    //new TypedValue(Convert.ToInt32(DxfCode.Start), "POLYLINE"),
                    new TypedValue(Convert.ToInt32(DxfCode.Start), "LWPOLYLINE"),
                    //new TypedValue(Convert.ToInt32(DxfCode.Start), "POLYLINE2D"),
                    //new TypedValue(Convert.ToInt32(DxfCode.Start), "POLYLINE3d"),
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "or>"),
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "and>")
                };

                SelectionFilter oSf = new SelectionFilter(tvs);

                oPSR = editor.SelectAll(oSf);

                if (oPSR.Status == PromptStatus.OK)
                {
                    retVal = oPSR.Value.GetObjectIds();
                }
                else
                {
                    retVal = new ObjectId[0];
                }
            }
            catch (Exception ex)
            {
                //ReportError(ex);
            }

            return retVal;
        }

        /// <summary>
        /// Get start and end points of entity
        /// </summary>
        /// <param name="WireEntity">Entity</param>
        /// <returns>Pair of start and end point of the entity</returns>
        /// <exception cref="Exception"></exception>
        public static (Point3d, Point3d) GetStartEndPoints(Entity WireEntity)
        {
            if (WireEntity == null)
                throw new Exception("Wire entity is null!");

            if (WireEntity is not Curve)
                throw new Exception("Wire entity is not curve!");

            var polyline = (Curve)WireEntity;

            return new(polyline.StartPoint, polyline.EndPoint);
        }

        /// <summary>
        /// Get extreme points of wire (connected to component and multiwire)
        /// </summary>
        /// <param name="multiWire">Multiwire</param>
        /// <param name="wireCurve">Wire</param>
        /// <returns>Point where wire connected to a Multiwire</returns>
        public static bool TryGetPointConnectedToMultiwire(Curve multiWire, Curve wireCurve, out Point3d output)
        {
            //var points = GetStartEndPoints(wireCurve);

            if (GeometryFunc.IsPointOnLine(multiWire, wireCurve.StartPoint))
            {
                output = wireCurve.StartPoint;
                return true;
            }

            if (GeometryFunc.IsPointOnLine(multiWire, wireCurve.EndPoint))
            {
                output = wireCurve.EndPoint;
                return true;
            }


            //throw new Exception("No cross-multiwire found!");
            output = new Point3d();
            return false;
        }

        /// <summary>
        /// Get all existed wires from database (lines on wires-layer)
        /// </summary>
        /// <param name="db">Autocad draft database</param>
        /// <returns>collection with curves</returns>
        public static IEnumerable<Curve> GetAllWiresFromDb(Database db)
        {
            var allWireCurves = new List<Curve>();
            // Get all lines and lwpolylines
            allWireCurves.AddRange(GetObjectsUtils.GetObjects<Line>(db, Layers.Wires));
            allWireCurves.AddRange(GetObjectsUtils.GetObjects<Polyline>(db, Layers.Wires).ToList());
            return allWireCurves;
        }
    }
}
