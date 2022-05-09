using AutocadCommands.Models;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using CommonHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutocadCommands.Models.IAutocadDirectionEnum;

namespace AutocadCommands.Services
{
    internal class WiresLinker : CommandPrototype
    {
        private PromptSelectionResult _selectionBlocks;
        private const string _multiWireLayer = "_MULTI_WIRE";
        private const string _wiresLayer = "_WIRES";
        private const string _wireNumbLayer = "WIRENO";
        private List<Entity> _multiWireEntities = new();
        private List<MultiWire> _multiWires = new();
        private double DoublePrecision = 0.1;

        private ObjectId[] SelectAllPolylineByLayer(string sLayer)
        {
            //Document oDwg = Application.DocumentManager.MdiActiveDocument;
            //Editor oEd = oDwg.Editor;

            ObjectId[] retVal = null;

            try
            {
                // Get a selection set of all possible polyline entities on the requested layer
                PromptSelectionResult oPSR = null;

                TypedValue[] tvs = new TypedValue[] {
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "<and"),
                    new TypedValue(Convert.ToInt32(DxfCode.LayerName), sLayer),
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "<or"),
                    //new TypedValue(Convert.ToInt32(DxfCode.Start), "POLYLINE"),
                    new TypedValue(Convert.ToInt32(DxfCode.Start), "LWPOLYLINE"),
                    //new TypedValue(Convert.ToInt32(DxfCode.Start), "POLYLINE2D"),
                    //new TypedValue(Convert.ToInt32(DxfCode.Start), "POLYLINE3d"),
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "or>"),
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "and>")
                };

                SelectionFilter oSf = new SelectionFilter(tvs);

                oPSR = _ed.SelectAll(oSf);

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

        public WiresLinker(Document doc) : base(doc)
        {
        }

        public override bool Init()
        {
            //Make the selection   
            var selectedItems = _ed.SelectImplied();

            if (selectedItems.Status == PromptStatus.OK)
            {
                var objectIds = selectedItems.Value.GetObjectIds();

                // Clear the PickFirst selection set
                var idarrayEmpty = new ObjectId[0];
                _ed.SetImpliedSelection(idarrayEmpty);

                _multiWireEntities = GetMultiWireEntities(objectIds).ToList();

                if (_multiWireEntities.Count != 0)
                    return true;
            }

            var promptResult = _ed.GetString("\nSelect polyline multiwires [All Select] <All>: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;

            var selectMethod = promptResult.StringResult?.ToUpper();
            if (selectMethod == null)
                return false;

            if (selectMethod == "A" || selectMethod == "А" || selectMethod == "")
            {
                _multiWireEntities = GetMultiWireEntities(SelectAllPolylineByLayer(_multiWireLayer)).ToList();
            }

            if(selectMethod == "S" || selectMethod == "Ы")
            {
                var filter = new SelectionFilter(
                new[]
                {
                    new TypedValue(0, "LWPOLYLINE")
                });

                var opts = new PromptSelectionOptions
                {
                    MessageForAdding = "Select polyline multiwires: "
                };

                //Make the selection   

                var selectionBlocks = _ed.GetSelection(opts, filter);
                if (selectionBlocks.Status != PromptStatus.OK)
                    return false;

                _multiWireEntities = GetMultiWireEntities(selectionBlocks.Value.GetObjectIds()).ToList();
            }

            return _multiWireEntities.Any();
        }

        private IEnumerable<Entity> GetMultiWireEntities(ObjectId[] objectIds)
        {
            using var acTrans = _db.TransactionManager.StartTransaction();
            foreach (ObjectId id in objectIds)
            {
                var entity = (Entity)acTrans.GetObject(id, OpenMode.ForRead);
                if (IsMultiWire(entity))
                    yield return entity;//_multiWireEntities.Add(entity);
            }
            _ed.WriteMessage("Number of objects in Pickfirst selection: " + _multiWireEntities.Count());
        }

        private static bool IsMultiWire(Entity entity)
        {
            //if(id.ObjectClass.DxfName == "LWPOLYLINE") it's the fastest!
            var entityType = entity.GetType();
            return entityType == typeof(Polyline) && entity.Layer.StartsWith(_multiWireLayer);
        }

        public override void Run()
        {
            _doc.LockDocument();
            CreateMultiWires(_multiWireEntities);
            //InsertNumbers(_multiWires);
            InsertBlocksWithNumbers(_multiWires);
            
        }

        private void InsertBlocksWithNumbers(List<MultiWire> multiWires)
        {
            foreach(var multiWire in multiWires)
            {

                foreach(var wire in multiWire.ConnectedWires)
                {
                    InsertBlock(wire);
                }
            }
        }

        private void InsertBlock(Wire wire)
        {
            if (IsHereAlreadyHaveText(wire))
                return;
            CreateText(wire);
            //BlockHelper.InsertBlock(_db, wire.PointConnectedToMultiWire, "WIRELINK_ABOVE");
        }

        private bool IsHereAlreadyHaveText(Wire wire)
        {
            var result = false;
            using (var tr = _db.TransactionManager.StartTransaction())
            {
                var model = (BlockTableRecord)tr.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(_db), OpenMode.ForRead);
                foreach (ObjectId id in model)
                {
                    if (id.ObjectClass.DxfName != "TEXT")
                        continue;
                    
                    var text = (DBText)tr.GetObject(id, OpenMode.ForRead);

                    if (text.Layer != _wireNumbLayer)
                        continue;

                    if(IsPoint3dEqual(text.AlignmentPoint, wire.TextCoordinate))
                    {
                        result = true;
                        _ed.WriteMessage($"\nText is already here = {text.TextString} ({text.AlignmentPoint})");
                    }
                        
                }
                tr.Commit();
            }
            return result;
        }

        private bool IsPoint3dEqual(Point3d first, Point3d second)
        {
            if (Math.Abs(first.X - second.X) < DoublePrecision &&
                Math.Abs(first.Y - second.Y) < DoublePrecision &&
                Math.Abs(first.Z - second.Z) < DoublePrecision)
                return true;

            return false;
        }

        private void CreateText(Wire wire)
        {
            // Get the current document and database

            // Start a transaction
            using Transaction acTrans = _db.TransactionManager.StartTransaction();
            // Open the Block table for read
            BlockTable acBlkTbl;
            acBlkTbl = acTrans.GetObject(_db.BlockTableId,
                                               OpenMode.ForRead) as BlockTable;
            // Open the Block table record Model space for write
            BlockTableRecord acBlkTblRec;
            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                  OpenMode.ForWrite) as BlockTableRecord;

            var acText = GenerateTexWiretNumbers(wire);

            acBlkTblRec.AppendEntity(acText);
            acTrans.AddNewlyCreatedDBObject(acText, true);
            // Save the changes and dispose of the transaction
            acTrans.Commit();
        }

        private DBText GenerateTexWiretNumbers( Wire wire)
        {
            // Create a single-line text object
            DBText acText = new DBText();
            acText.SetDatabaseDefaults();

            acText.Layer = _wireNumbLayer;
            acText.Height = 2.0;
            acText.WidthFactor = 0.8;
            acText.TextString = "-";
            SetTextDirection(acText, wire);
            
            return acText;
        }

        private void SetTextDirection(DBText acText, Wire wire)
        {
            switch (wire.Direction)
            { 
                case Direction.Above:
                    acText.Justify = AttachmentPoint.BottomRight;
                    acText.AlignmentPoint = wire.TextCoordinate;
                    break;

                case Direction.Left:
                    acText.Justify = AttachmentPoint.BottomRight;
                    acText.AlignmentPoint = wire.TextCoordinate;
                    break;

                case Direction.Below:
                    acText.Justify = AttachmentPoint.TopRight;
                    acText.AlignmentPoint = wire.TextCoordinate;
                    break;

                case Direction.Right:
                    acText.Justify = AttachmentPoint.BottomLeft;
                    acText.AlignmentPoint = wire.TextCoordinate;
                    break;
            }
        }

        private void InsertNumbers(IEnumerable<MultiWire> multiWires)
        {
            foreach(var multiWire in multiWires)
            {
                var NumbOfWires = multiWire.ConnectedWires.Count;
                if (NumbOfWires % 2 == 0) // кол-во проводов четное
                {
                    
                }
            }
        }

        private void CreateMultiWires(IEnumerable<Entity> multiWireEntities)
        {
            foreach (var multiWire in multiWireEntities)
            {
                var connectedWires = FindAllConnectedWires(multiWire);
                if (connectedWires.Any())
                {
                    _multiWires.Add(new MultiWire()
                    {
                        MultiWireEntity = multiWire, ConnectedWires = connectedWires.ToList()
                    });
                    
                }
                _ed.WriteMessage("\nNumber of wires: " + connectedWires.Count());

            }
        }

        private IEnumerable<Wire> FindAllConnectedWires(Entity multiWireEntity)
        {
            var allWires = GetAllPolyWiresFromSheet();
            foreach (var wireEntity in allWires)
            {
                var wire = GetWireConnectedToMultiWire(multiWireEntity, wireEntity);
                if (wire != null)
                {
                    yield return wire;
                }
            }
        }

        private Wire GetWireConnectedToMultiWire(Entity MultiWireEntity, Entity wireEntity)
        {

            //Point3dCollection vertices = GetAllVertices(wire);
            var polyLine = (Polyline)wireEntity;

            if(IsPointOnPolyline((Polyline)MultiWireEntity, polyLine.StartPoint))
            {
                var nearPoint = polyLine.GetPoint3dAt(1);
                return new Wire()
                {
                    WireEntity = wireEntity,
                    Direction = GetDirection(polyLine.StartPoint, nearPoint),
                    PointConnectedToMultiWire = polyLine.StartPoint
                };
            }
            if(IsPointOnPolyline((Polyline)MultiWireEntity, polyLine.EndPoint))
            {
                var nearPoint = polyLine.GetPoint3dAt(polyLine.NumberOfVertices - 2);
                return new Wire()
                {
                    WireEntity = wireEntity,
                    Direction = GetDirection(polyLine.EndPoint, nearPoint),
                    PointConnectedToMultiWire = polyLine.EndPoint
                };
            }
            return null;
        }

        private Direction GetDirection(Point3d startPoint, Point3d endPoint)
        {
            double angleRad = Math.Atan2(startPoint.X - endPoint.X, startPoint.Y - endPoint.Y);
            var pi = Math.PI;
            _ed.WriteMessage("\nX1: " + startPoint.X.ToString() + "X2: " + endPoint.X.ToString());
            _ed.WriteMessage("Y1: " + startPoint.Y.ToString() + "Y2: " + endPoint.Y.ToString());
            _ed.WriteMessage(" -> ");

            if ((angleRad >= 3* pi / 4 && angleRad <= pi) || (angleRad >= -pi && angleRad < -3*pi/4))
            {
                _ed.WriteMessage("Direction of wire: " + angleRad.ToString() + " = Above");
                return Direction.Above;
            }
                

            if (angleRad >= pi / 4 && angleRad < 3 * pi / 4)
            { 
                _ed.WriteMessage("Direction of wire: " + angleRad.ToString() + " = Left");
                return Direction.Left;
            }

            if (angleRad <= pi / 4 && angleRad > -pi / 4)
            {
                _ed.WriteMessage("Direction of wire: " + angleRad.ToString() + " = Below");
                return Direction.Below;
            }

            if (angleRad <= -pi / 4 && angleRad >= -3*pi / 4)
            { 
                _ed.WriteMessage("Direction of wire: " + angleRad.ToString() + " = Right");
                return Direction.Right;
            }
            _ed.WriteMessage("Direction of wire: " + angleRad.ToString() + " = Unknown");
            return Direction.Above;
        }

        private Point3dCollection GetAllVertices(Entity wire)
        {
            var verticesCollection = new Point3dCollection();
            var polyline = (Polyline)wire;
            for (var i = 0; i < polyline.NumberOfVertices - 1; i++)
            {
                verticesCollection.Add(polyline.StartPoint);
                verticesCollection.Add(polyline.EndPoint);
            }

            return verticesCollection;
        }

        private IEnumerable<Entity> GetAllPolyWiresFromSheet()
        {
            var wires = SelectAllPolylineByLayer(_wiresLayer);
            using var acTrans = _db.TransactionManager.StartTransaction();
            foreach (ObjectId wireId in wires)
            {
                yield return (Entity)acTrans.GetObject(wireId, OpenMode.ForRead);
            }
        }

        private bool IsPointOnPolyline(Polyline pl, Point3d pt)
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
    }
}
