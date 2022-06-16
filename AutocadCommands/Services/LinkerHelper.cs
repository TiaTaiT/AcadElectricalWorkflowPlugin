using AutocadCommands.Helpers;
using AutocadCommands.Models;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutocadCommands.Models.IAutocadDirectionEnum;

namespace AutocadCommands.Services
{
    public static class LinkerHelper
    {
        private const string _sourceLeft = "ha2s1_inline";
        private const string _sourceDown = "ha2s2_inline";
        private const string _sourceRight = "ha2s3_inline";
        private const string _sourceUp = "ha2s4_inline";

        private const string _destinationLeft = "ha2d1_inline";
        private const string _destinationDown = "ha2d2_inline";
        private const string _destinationRight = "ha2d3_inline";
        private const string _destinationUp = "ha2d4_inline";

        private const double _precision = 0.1;

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

        private static Direction GetDirection(Point3d startPoint, Point3d endPoint)
        {
            double angleRad = Math.Atan2(startPoint.X - endPoint.X, startPoint.Y - endPoint.Y);
            var pi = Math.PI;
            
            if ((angleRad >= 3 * pi / 4 && angleRad <= pi) || (angleRad >= -pi && angleRad < -3 * pi / 4))
            {
                return Direction.Above;
            }

            if (angleRad >= pi / 4 && angleRad < 3 * pi / 4)
            {
                return Direction.Left;
            }

            if (angleRad <= pi / 4 && angleRad > -pi / 4)
            {
                return Direction.Below;
            }

            if (angleRad <= -pi / 4 && angleRad >= -3 * pi / 4)
            {
                return Direction.Right;
            }

            return Direction.Above;
        }

        public static IEnumerable<ObjectId> GetAllSourceDestinationSymbols(Database db, Polyline multiwire)
        {
            using var tr = db.TransactionManager.StartTransaction();
            var btl = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
            foreach (var item in btl)
            {
                var bk = (BlockTableRecord)tr.GetObject(item, OpenMode.ForRead);
                
                if (!bk.IsLayout)
                    continue;

                foreach (var obj in bk)
                {
                    Entity ent = (Entity)tr.GetObject(obj, OpenMode.ForWrite);
                    
                    if (ent == null || ent.GetType() != typeof(BlockReference))
                        continue;

                    var br = (BlockReference)tr.GetObject(obj, OpenMode.ForRead);

                    if (!IsNameOfSourceBlock(br.Name))
                        continue;

                    if (IsPointOnPolyline(multiwire, br.Position))
                    {
                        yield return obj;
                    }
                }
            }
            tr.Commit();
        }
        
        private static Entity GetSignalSymbol<T>(Database db, T pline)
        {
            if (pline is Line line)
            {
                var signalBlockId1 = GetSignalEntity(db, line.StartPoint);
                if(signalBlockId1 != null)
                    return signalBlockId1;
                return GetSignalEntity(db, line.EndPoint);
            }
            if(pline is Polyline polyline)
            {
                var signalBlockId1 = GetSignalEntity(db, polyline.StartPoint);
                if (signalBlockId1 != null)
                    return signalBlockId1;
                return GetSignalEntity(db, polyline.EndPoint);
            }
            throw new Exception("symbol not found");
        }

        private static Entity GetSignalEntity(Database db, Point3d point3d)
        {
            using var tr = db.TransactionManager.StartTransaction();
            var btl = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
            foreach (var item in btl)
            {
                var bk = (BlockTableRecord)tr.GetObject(item, OpenMode.ForRead);

                if (!bk.IsLayout)
                    continue;

                foreach (var obj in bk)
                {
                    Entity ent = (Entity)tr.GetObject(obj, OpenMode.ForWrite);

                    if (ent == null || ent.GetType() != typeof(BlockReference))
                        continue;

                    var br = (BlockReference)tr.GetObject(obj, OpenMode.ForRead);

                    if (!IsNameOfSourceBlock(br.Name))
                        continue;

                    if (IsPoint3dEqual(br.Position, point3d))
                        continue;

                    return ent;
                }
            }
            tr.Commit();
            return null;
        }
        
        private static bool IsNameOfSourceBlock(string name)
        {
            return name switch
            {
                _sourceLeft => true,
                _sourceRight => true,
                _sourceDown => true,
                _sourceUp => true,
                _ => false,
            };
        }

        /// <summary>
        /// Get all Ids of objects that contain one of SIGCODE attribute from the fullWires SIGCODEs
        /// </summary>
        /// <param name="db">Autocad draft database</param>
        /// <param name="fullWires">FullWires with SIGCODE</param>
        /// <returns>ObjectIds</returns>
        public static IEnumerable<Wire> GetAllRelatedWires(Database db, Transaction tr, Entity multiwire, IEnumerable<FullWire> fullWires)
        {
            
            var sigCodesForSearch = GetSigCodeValues(tr, fullWires);
            Debug.WriteLine("sigCodesForSearch = " + sigCodesForSearch.Count());
            var objectIds = FoundBlockReferencesIdsWithSigCodes(db, tr, sigCodesForSearch);
            Debug.WriteLine("objectIds = " + objectIds.Count());
            var result = new List<Wire>();
            foreach (var objectId in objectIds) 
            {
                Debug.WriteLine("objectId = " + objectId.IsNull.ToString());
                var connectedWire = GetWireConnectedToMultiWire(db, multiwire, (Entity)objectId.GetObject(OpenMode.ForRead, false));
                
                if(connectedWire != null)
                    result.Add(connectedWire);
            }
            return result;
        }

        private static IEnumerable<ObjectId> FoundBlockReferencesIdsWithSigCodes(Database db, Transaction tr, IEnumerable<string> sigCodesForSearch)
        {
            var objectIds = new List<ObjectId>();
            foreach(var sigCode in sigCodesForSearch)
            {
                objectIds.AddRange(AttributeHelper.GetObjectsWithAttribute(db, tr, "SIGCODE", sigCode));
            }
            return objectIds;
        }

        private static IEnumerable<string> GetSigCodeValues(Transaction tr, IEnumerable<FullWire> fullWires)
        {
            var sigCodes = new List<string>();
            foreach (var fullWire in fullWires)
            {
                if (fullWire.SourceWire.SignalSymbol != null && fullWire.DestinationWire.SignalSymbol != null)
                {
                    sigCodes.Add(AttributeHelper.GetAttributeValueFromBlock(tr, fullWire.SourceWire.SignalSymbol.Id, "SIGCODE"));
                    sigCodes.Add(AttributeHelper.GetAttributeValueFromBlock(tr, fullWire.DestinationWire.SignalSymbol.Id, "SIGCODE"));
                }
                
            }
            return sigCodes;
        }

        public static IEnumerable<string> GetExistSourceNumbers(Transaction tr, IEnumerable<ObjectId> blockIds)
        {
            var descriptions = new List<string>();
            foreach (var blockId in blockIds)
            {
                descriptions.Add(AttributeHelper.GetAttributeValueFromBlock(tr, blockId, "DESC1"));
            }
            return descriptions;
        }

        public static bool IsPoint3dEqual(Point3d first, Point3d second)
        {
            if (Math.Abs(first.X - second.X) < _precision &&
                Math.Abs(first.Y - second.Y) < _precision &&
                Math.Abs(first.Z - second.Z) < _precision)
                return true;

            return false;
        }

        public static Wire GetWireConnectedToMultiWire(Database db, Entity MultiWireEntity, Entity wireEntity)
        {
            if (wireEntity is Polyline polyLine)
            {
                if (IsPointOnPolyline((Polyline)MultiWireEntity, polyLine.StartPoint))
                {
                    var nearPoint = polyLine.GetPoint3dAt(1);
                    return new Wire()
                    {
                        WireEntity = wireEntity,
                        Direction = GetDirection(polyLine.StartPoint, nearPoint),
                        PointConnectedToMultiWire = polyLine.StartPoint,
                        SignalSymbol = GetSignalSymbol(db, polyLine)
                    };
                }
                if (IsPointOnPolyline((Polyline)MultiWireEntity, polyLine.EndPoint))
                {
                    var nearPoint = polyLine.GetPoint3dAt(polyLine.NumberOfVertices - 2);
                    return new Wire()
                    {
                        WireEntity = wireEntity,
                        Direction = GetDirection(polyLine.EndPoint, nearPoint),
                        PointConnectedToMultiWire = polyLine.EndPoint,
                        SignalSymbol = GetSignalSymbol(db, polyLine)
                    };
                }
            }
            else if (wireEntity is Line line)
            {
                if (IsPointOnPolyline((Polyline)MultiWireEntity, line.StartPoint))
                {
                    return new Wire()
                    {
                        WireEntity = wireEntity,
                        Direction = GetDirection(line.StartPoint, line.EndPoint),
                        PointConnectedToMultiWire = line.StartPoint,
                        SignalSymbol = GetSignalSymbol(db, line)
                    };
                }
                if (IsPointOnPolyline((Polyline)MultiWireEntity, line.EndPoint))
                {
                    return new Wire()
                    {
                        WireEntity = wireEntity,
                        Direction = GetDirection(line.EndPoint, line.StartPoint),
                        PointConnectedToMultiWire = line.EndPoint,
                        SignalSymbol = GetSignalSymbol(db, line)
                    };
                }
            }
            return null;
        }

        public static Polyline GetConnectedMultiwire(Entity sourceEntity, Entity destinationEntity, IEnumerable<Polyline> multiwire)
        {
            var source = sourceEntity;
            var destination = destinationEntity;
            var polylines = multiwire;

            Point3d sourceStartPoint = GetStartPoint(source);
            Point3d sourceEndPoint = GetEndPoint(source);

            Point3d destinationStartPoint = GetStartPoint(destination);
            Point3d destinationEndPoint = GetEndPoint(destination);

            foreach(var polyline in polylines)
            {
                // we must check that every wire in couple at one end connected to the same multiwire 
                
                var isSourceEntityConnected = IsPointOnPolyline(polyline, sourceStartPoint) ^ IsPointOnPolyline(polyline, sourceEndPoint);
                var isDestEntityConnected = IsPointOnPolyline(polyline, destinationStartPoint) ^ IsPointOnPolyline(polyline, destinationEndPoint);

                var isBothEntityConnected = isSourceEntityConnected & isDestEntityConnected;

                if(isBothEntityConnected) 
                    return polyline;
            }
            return null;
        }

        private static Point3d GetEndPoint(Entity sourceEntity)
        {
            if (sourceEntity is Polyline polyline)
            {
                return polyline.EndPoint;
            }
            if (sourceEntity is Line line)
            {
                return line.EndPoint;
            }
            throw new ArgumentException("Source or destination wire aren't line or polyline type");
        }

        private static Point3d GetStartPoint(Entity sourceEntity) 
        {
            if(sourceEntity is Polyline polyline)
            {
                return polyline.StartPoint;
            }
            if(sourceEntity is Line line)
            {
                return line.StartPoint;
            }
            throw new ArgumentException("Source or destination wire aren't line or polyline type");
        }

        public static bool IsPointOnPolyline(Polyline pl, Point3d pt)
        {
            bool isOn = false;
            for (int i = 0; i < pl.NumberOfVertices; i++)
            {
                Curve3d seg = null;

                SegmentType segType = pl.GetSegmentType(i);
                if (segType == SegmentType.Arc)
                    seg = pl.GetArcSegmentAt(i);
                else if (segType == SegmentType.Line)
                    seg = pl.GetLineSegmentAt(i);

                if (seg != null)
                {
                    isOn = seg.IsOn(pt);
                    if (isOn)
                        break;
                }
            }
            return isOn;
        }

        public static bool EraseBlockIfExist(Database db, Wire wire)
        {
            var result = false;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var btl = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
                foreach (var item in btl)
                {
                    var bk = (BlockTableRecord)tr.GetObject(item, OpenMode.ForRead);
                    if (bk.IsLayout)
                    {
                        foreach (var obj in bk)
                        {
                            Entity ent = (Entity)tr.GetObject(obj, OpenMode.ForWrite);
                            if (ent != null && ent.GetType() == typeof(BlockReference))
                            {
                                var br = (BlockReference)tr.GetObject(obj, OpenMode.ForRead);
                                if (IsPoint3dEqual(br.Position, wire.PointConnectedToMultiWire))
                                {
                                    ent.Erase();
                                    result = true;
                                }
                            }
                        }
                    }
                }
                tr.Commit();
            }
            return result;
        }

        public static string GetBlockNameByWireDirectionSource(Wire wire)
        {
            if (wire.Direction == Direction.Left)
                return _sourceLeft;
            if (wire.Direction == Direction.Below)
                return _sourceDown;
            if (wire.Direction == Direction.Right)
                return _sourceRight;
            return _sourceUp;
        }

        public static string GetBlockNameByWireDirectionDestination(Wire wire)
        {
            if (wire.Direction == Direction.Left)
                return _destinationLeft;
            if (wire.Direction == Direction.Below)
                return _destinationDown;
            if (wire.Direction == Direction.Right)
                return _destinationRight;
            return _destinationUp;
        }
    }
}
