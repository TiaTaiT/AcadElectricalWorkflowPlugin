using AutocadCommands.Models;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using CommonHelpers;
using CommonHelpers.Model;
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
        private const string _wiresLayer = "WIRES";
        private const string _wireNumbLayer = "WIRENO";

        private const string _sourceLeft = "ha2s1_inline";
        private const string _sourceDown = "ha2s2_inline";
        private const string _sourceRight = "ha2s3_inline";
        private const string _sourceUp = "ha2s4_inline";

        private const string _destinationLeft = "ha2d1_inline";
        private const string _destinationDown = "ha2d2_inline";
        private const string _destinationRight = "ha2d3_inline";
        private const string _destinationUp = "ha2d4_inline";

        private List<Entity> _multiWireEntities = new();
        private List<MultiWire> _multiWires = new();

        private double DoublePrecision = 0.1;


        private ObjectId[] SelectAllLineByLayer(string layer)
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
                    new TypedValue(Convert.ToInt32(DxfCode.LayerName), layer),
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "<or"),
                    //new TypedValue(Convert.ToInt32(DxfCode.Start), "POLYLINE"),
                    new TypedValue(Convert.ToInt32(DxfCode.Start), "LINE"),
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

        private ObjectId[] SelectAllPolylineByLayer(string layer)
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
                    new TypedValue[] {
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "<and"),
                    new TypedValue(Convert.ToInt32(DxfCode.LayerName), _multiWireLayer),
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "<or"),
                    new TypedValue(Convert.ToInt32(DxfCode.Start), "LWPOLYLINE"),
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "or>"),
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "and>")
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
                if (multiWire.ConnectedWires.Count % 2 != 0)
                {
                    _ed.WriteMessage($"\nWarning! An odd number of connections ({multiWire.ConnectedWires.Count}) to multiwire were found.");
                }

                foreach (var wire in multiWire.ConnectedWires)
                {

                    InsertTextBlock(wire);
                }
                /* Вставка блоков (не работает)
                var sourceDestPairs = multiWire.GetSourceDestinationWirePairs();
                foreach(var wiresPair in sourceDestPairs)
                {
                    CreateWireLinks(wiresPair.sourceWire, wiresPair.destinationWire);
                }
                */
            }
        }

        private void InsertTextBlock(Wire wire)
        {
            if (IsHereAlreadyHaveText(wire))
                return;
            CreateText(wire);  
        }

        private int counter = 0;
        private void CreateWireLinks(Wire sourceWire, Wire destinationWire)
        {

            var attributes = new List<FakeAttribute>
            {
                /*
                   Attribute:               -   Layer:

                   X8_TINY_DOT_DONT_REMOVE	-	MISC
                   X2_TINY_DOT_DONT_REMOVE	-	MISC
                   X2TERM02				    -	MISC
                   X8TERM01				    -	MISC
                   SIGCODE					-	MISC
                   XREF				    	-	XREF
                   DESC1					-	DESC
                   WIRENO					-	WIREREF
                */
                new FakeAttribute{Tag = "X8_TINY_DOT_DONT_REMOVE", Layer = "MISC"},
                new FakeAttribute{Tag = "X2_TINY_DOT_DONT_REMOVE", Layer = "MISC"},
                new FakeAttribute{Tag = "X2TERM02", Layer = "MISC"},
                new FakeAttribute{Tag = "X2TERM01", Layer = "MISC"},
                new FakeAttribute{Tag = "SIGCODE", Value = counter.ToString(), Layer = "MISC"},
                new FakeAttribute{Tag = "XREF", Value = "", Layer = "XREF"},
                new FakeAttribute{Tag = "DESC1", Value = counter.ToString(), Layer = "DESC"},
                new FakeAttribute{Tag = "WIRENO", Value = "", Layer = "WIREREF"},
            };

            var sourceBlockName = GetBlockNameByWireDirectionSource(sourceWire);
            if(!IsHereAlreadyHaveBlock(sourceWire))
                BlockUtils.InsertBlockFormFile(_db, sourceBlockName, sourceWire.PointConnectedToMultiWire, attributes);

            var destinationBlockName = GetBlockNameByWireDirectionDestination(destinationWire);
            if (!IsHereAlreadyHaveBlock(sourceWire))
                BlockUtils.InsertBlockFormFile(_db, destinationBlockName, destinationWire.PointConnectedToMultiWire, attributes);
            counter++;
        }

        private string GetBlockNameByWireDirectionSource(Wire wire)
        {
            if (wire.Direction == Direction.Left)
                return _sourceLeft;
            if (wire.Direction == Direction.Below)
                return _sourceDown;
            if (wire.Direction == Direction.Right)
                return _sourceRight;
            return _sourceUp;
        }

        private string GetBlockNameByWireDirectionDestination(Wire wire)
        {
            if (wire.Direction == Direction.Left)
                return _destinationLeft;
            if (wire.Direction == Direction.Below)
                return _destinationDown;
            if (wire.Direction == Direction.Right)
                return _destinationRight;
            return _destinationUp;
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

        private bool IsHereAlreadyHaveBlock(Wire wire)
        {
            var result = false;
            using (var tr = _db.TransactionManager.StartTransaction())
            {
                var btl = (BlockTable)_db.BlockTableId.GetObject(OpenMode.ForRead);
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
                                    _ed.WriteMessage("\nBlock Name is here: (" + br.Name + ");" + "X = " + br.Position.X + "Y = " + br.Position.Y);
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
            acBlkTbl = acTrans.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
            // Open the Block table record Model space for write
            BlockTableRecord acBlkTblRec;
            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            var acText = GenerateTexWireNumbers(wire);

            acBlkTblRec.AppendEntity(acText);
            acTrans.AddNewlyCreatedDBObject(acText, true);
            // Save the changes and dispose of the transaction
            acTrans.Commit();
        }

        private DBText GenerateTexWireNumbers( Wire wire)
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
            var allWires = GetAllWiresFromSheet();
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
            if(wireEntity is Polyline polyLine)
            //var line = (Line)wireEntity;
            {
                if (IsPointOnPolyline((Polyline)MultiWireEntity, polyLine.StartPoint))
                {
                    var nearPoint = polyLine.GetPoint3dAt(1);
                    return new Wire()
                    {
                        WireEntity = wireEntity,
                        Direction = GetDirection(polyLine.StartPoint, nearPoint),
                        PointConnectedToMultiWire = polyLine.StartPoint
                    };
                }
                if (IsPointOnPolyline((Polyline)MultiWireEntity, polyLine.EndPoint))
                {
                    var nearPoint = polyLine.GetPoint3dAt(polyLine.NumberOfVertices - 2);
                    return new Wire()
                    {
                        WireEntity = wireEntity,
                        Direction = GetDirection(polyLine.EndPoint, nearPoint),
                        PointConnectedToMultiWire = polyLine.EndPoint
                    };
                }
            }
            else if(wireEntity is Line line)
            {
                if (IsPointOnPolyline((Polyline)MultiWireEntity, line.StartPoint))
                {

                    return new Wire()
                    {
                        WireEntity = wireEntity,
                        Direction = GetDirection(line.StartPoint, line.EndPoint),
                        PointConnectedToMultiWire = line.StartPoint
                    };
                }
                if (IsPointOnPolyline((Polyline)MultiWireEntity, line.EndPoint))
                {
                    
                    return new Wire()
                    {
                        WireEntity = wireEntity,
                        Direction = GetDirection(line.EndPoint, line.StartPoint),
                        PointConnectedToMultiWire = line.EndPoint
                    };
                }
            }
            return null;
        }

        private Direction GetDirection(Point3d startPoint, Point3d endPoint)
        {
            double angleRad = Math.Atan2(startPoint.X - endPoint.X, startPoint.Y - endPoint.Y);
            var pi = Math.PI;
            _ed.WriteMessage("\nX1: " + startPoint.X.ToString() + "; X2: " + endPoint.X.ToString());
            _ed.WriteMessage("Y1: " + startPoint.Y.ToString() + "; Y2: " + endPoint.Y.ToString());
            _ed.WriteMessage(" -> ");

            if ((angleRad >= 3* pi / 4 && angleRad <= pi) || (angleRad >= -pi && angleRad < -3*pi/4))
            {
                _ed.WriteMessage("Direction of wire: " + angleRad.ToString() + " = Above ");
                return Direction.Above;
            }
                

            if (angleRad >= pi / 4 && angleRad < 3 * pi / 4)
            { 
                _ed.WriteMessage("Direction of wire: " + angleRad.ToString() + " = Left ");
                return Direction.Left;
            }

            if (angleRad <= pi / 4 && angleRad > -pi / 4)
            {
                _ed.WriteMessage("Direction of wire: " + angleRad.ToString() + " = Below ");
                return Direction.Below;
            }

            if (angleRad <= -pi / 4 && angleRad >= -3*pi / 4)
            { 
                _ed.WriteMessage("Direction of wire: " + angleRad.ToString() + " = Right ");
                return Direction.Right;
            }
            _ed.WriteMessage("Direction of wire: " + angleRad.ToString() + " = Unknown ");
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

        private IEnumerable<Entity> GetAllWiresFromSheet()
        {
            // Get all lines and lwpolylines
            var wires = SelectAllPolylineByLayer(_wiresLayer).ToList();
            wires.AddRange(SelectAllLineByLayer(_wiresLayer).ToList());

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
