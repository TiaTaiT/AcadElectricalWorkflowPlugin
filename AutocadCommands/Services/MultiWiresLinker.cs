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
    internal class MultiWiresLinker : CommandPrototype
    {
        private PromptSelectionResult _selectionBlocks;
        private const string _multiWireLayer = "_MULTI_WIRE";
        private const string _wiresLayer = "WIRES";
        private const string _wireNumbLayer = "WIRENO";
        private const string _symbolsLayer = "SYMS";

        private List<Entity> _multiWireEntities = new();
        private List<MultiWire> _multiWires = new();

        


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

        

        public MultiWiresLinker(Document doc) : base(doc)
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
                _multiWireEntities = GetMultiWireEntities(LinkerHelper.SelectAllPolylineByLayer(_ed, _multiWireLayer)).ToList();
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
                    yield return entity;
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
            CreateMultiWires(_db, _multiWireEntities);
            InsertBlocksWithNumbers(_multiWires);
            
        }

        private void CreateMultiWires(Database db, IEnumerable<Entity> multiWireEntities)
        {
            foreach (var multiWire in multiWireEntities)
            {
                var connectedWires = FindAllConnectedWires(db, multiWire);
                if (connectedWires.Any())
                {
                    _multiWires.Add(new MultiWire()
                    {
                        MultiWireEntity = multiWire,
                        ConnectedWires = connectedWires.ToList()
                    });

                }
                _ed.WriteMessage("\nNumber of wires: " + connectedWires.Count());

            }
        }

        private void InsertBlocksWithNumbers(List<MultiWire> multiWires)
        {
            foreach(var multiWire in multiWires)
            {
                if (multiWire.ConnectedWires.Count % 2 != 0)
                {
                    _ed.WriteMessage($"\nWarning! An odd number of connections ({multiWire.ConnectedWires.Count}) to multiwire were found.");
                }

                var fullWires = multiWire.GetSourceDestinationWirePairs();

                CreateWireLinks(fullWires);
            }
        }

        private void CreateWireLinks(IEnumerable<FullWire> fullWires)
        {
            int counter = 1;

            foreach (var fullWire in fullWires)
            {
                var sourceWire = fullWire.SourceWire;
                var destinationWire = fullWire.DestinationWire;
                var attributes = new List<FakeAttribute>
                {
                    new FakeAttribute{Tag = "X8_TINY_DOT_DONT_REMOVE", Layer = "MISC"},
                    new FakeAttribute{Tag = "X2_TINY_DOT_DONT_REMOVE", Layer = "MISC"},
                    new FakeAttribute{Tag = "X2TERM02", Layer = "MISC"},
                    new FakeAttribute{Tag = "X2TERM01", Layer = "MISC"},
                    new FakeAttribute{Tag = "SIGCODE", Value = Guid.NewGuid().ToString(), Layer = "MISC"},
                    new FakeAttribute{Tag = "XREF", Value = "", Layer = "XREF"},
                    new FakeAttribute{Tag = "DESC1", Value = counter.ToString(), Layer = "DESC"},
                    new FakeAttribute{Tag = "WIRENO", Value = "", Layer = "WIREREF"},
                };

                    var sourceBlockName = LinkerHelper.GetBlockNameByWireDirectionSource(sourceWire);
                    LinkerHelper.EraseBlockIfExist(_db, sourceWire);
                    BlockUtils.InsertBlockFormFile(_db, sourceBlockName, sourceWire.PointConnectedToMultiWire, attributes, _symbolsLayer);

                    var destinationBlockName = LinkerHelper.GetBlockNameByWireDirectionDestination(destinationWire);
                    LinkerHelper.EraseBlockIfExist(_db, destinationWire);
                    BlockUtils.InsertBlockFormFile(_db, destinationBlockName, destinationWire.PointConnectedToMultiWire, attributes, _symbolsLayer);

                    counter++;
                }
            
        }

        private IEnumerable<Wire> FindAllConnectedWires(Database db, Entity multiWireEntity)
        {
            var allWires = GetAllWiresFromSheet();
            foreach (var wireEntity in allWires)
            {
                var wire = LinkerHelper.GetWireConnectedToMultiWire(db, multiWireEntity, wireEntity);
                if (wire != null)
                {
                    yield return wire;
                }
            }
        }

        private IEnumerable<Entity> GetAllWiresFromSheet()
        {
            // Get all lines and lwpolylines
            var wires = LinkerHelper.SelectAllPolylineByLayer(_ed, _wiresLayer).ToList();
            wires.AddRange(SelectAllLineByLayer(_wiresLayer).ToList());

            using var acTrans = _db.TransactionManager.StartTransaction();
            foreach (ObjectId wireId in wires)
            {
                yield return (Entity)acTrans.GetObject(wireId, OpenMode.ForRead);
            }
        }
    }
}
