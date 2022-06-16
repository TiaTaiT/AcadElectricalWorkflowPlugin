using AutocadCommands.Models;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using CommonHelpers;
using CommonHelpers.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutocadCommands.Services
{
    public class WiresLinker : CommandPrototype
    {
        private const string _multiWireLayer = "_MULTI_WIRE";
        private const string _wiresLayer = "WIRES";
        private const string _wireNumbLayer = "WIRENO";

        private List<(ObjectId, ObjectId)> _sourceDestPairIds = new();
        private List<FullWire> _fullWires = new();
        private Polyline connectedMultiwire; // bus polyline

        public WiresLinker(Document doc) : base(doc)
        {
        }

        public override bool Init()
        {
            var escape = false;

            while(!escape)
            {
                var options1 = new PromptEntityOptions("\nSelect source wire: ");
                options1.SetRejectMessage("not valid Object \n");
                options1.AddAllowedClass(typeof(Line), true);
                options1.AddAllowedClass(typeof(Polyline), true);

                //Make the selection   
                var selectionBlock1 = _ed.GetEntity(options1);//_ed.GetSelection(opts, filter);
                if (selectionBlock1.Status != PromptStatus.OK)
                {
                    if (_sourceDestPairIds.Count == 0)
                        return false;
                    else return true;
                }
                    
                var sourceWireId = selectionBlock1.ObjectId;

                HighlightObject(sourceWireId);

                var options2 = new PromptEntityOptions("\nSelect destination wire: ");
                options2.SetRejectMessage("not valid Object \n");
                options2.AddAllowedClass(typeof(Line), true);
                options2.AddAllowedClass(typeof(Polyline), true);
                //Make the selection   
                var selectionBlock2 = _ed.GetEntity(options2);//_ed.GetSelection(opts, filter);
                if (selectionBlock2.Status != PromptStatus.OK)
                {
                    if (_sourceDestPairIds.Count == 0)
                        return false;
                    else return true;
                }
                
                var destinationWireId = selectionBlock2.ObjectId;

                if (sourceWireId == destinationWireId)
                {
                    _ed.WriteMessage("Source and destination wire are the same!");
                    return false;
                }

                HighlightObject(destinationWireId);

                _sourceDestPairIds.Add((sourceWireId, destinationWireId));       
            }
            return true;
        }

        private void HighlightObject(ObjectId sourceWireId)
        {
            using var tr = _db.TransactionManager.StartTransaction();
            Entity ent = (Entity)tr.GetObject(sourceWireId, OpenMode.ForWrite);
            ent.Highlight();
        }

        private void UnhighlightObject(ObjectId sourceWireId)
        {
            using var tr = _db.TransactionManager.StartTransaction();
            Entity ent = (Entity)tr.GetObject(sourceWireId, OpenMode.ForWrite);
            ent.Unhighlight();
        }

        public override void Run()
        {
            _doc.LockDocument();
            using var tr = _db.TransactionManager.StartTransaction();
            TieWirePairs();
            var relatedWires = LinkerHelper.GetAllRelatedWires(_db, tr, connectedMultiwire, _fullWires);
            Debug.WriteLine("relatedWires count: " + relatedWires.Count());
            //EraseExistingFullWireSymbols(_fullWires);
            //EraseExistingWireSymbols(relatedWires);

            //var linkBlocks = LinkerHelper.GetAllSourceDestinationSymbols(_db, connectedMultiwire);

            //var numberStrings = LinkerHelper.GetExistSourceNumbers(tr, linkBlocks);

            //PutSourceDestinationSymbols(ConvertStringsToInts(numberStrings));
            UnhighlightAllPassedObjects();
        }

        private void UnhighlightAllPassedObjects()
        {
            foreach(var pair in _sourceDestPairIds)
            {
                UnhighlightObject(pair.Item1);
                UnhighlightObject(pair.Item2);
            }
        }

        private void EraseExistingFullWireSymbols(IEnumerable<FullWire> fullWires)
        {
            foreach (var fwire in fullWires)
            {
                LinkerHelper.EraseBlockIfExist(_db, fwire.SourceWire);
                LinkerHelper.EraseBlockIfExist(_db, fwire.DestinationWire);
            }
        }

        private void EraseExistingWireSymbols(IEnumerable<Wire> wires)
        {
            _ed.WriteMessage("\n" + wires.Count());
            foreach (var wire in wires)
            {
                LinkerHelper.EraseBlockIfExist(_db, wire);
            }
        }

        private void PutSourceDestinationSymbols(IEnumerable<int> existingLinkNumbers)
        {
            foreach (var fullWire in _fullWires)
            {
                var freeLinkNumber = GetFreeNumb(existingLinkNumbers);
                var attributes = new List<FakeAttribute>
                {
                    new FakeAttribute{Tag = "X8_TINY_DOT_DONT_REMOVE", Layer = "MISC"},
                    new FakeAttribute{Tag = "X2_TINY_DOT_DONT_REMOVE", Layer = "MISC"},
                    new FakeAttribute{Tag = "X2TERM02", Layer = "MISC"},
                    new FakeAttribute{Tag = "X2TERM01", Layer = "MISC"},
                    new FakeAttribute{Tag = "SIGCODE", Value = Guid.NewGuid().ToString(), Layer = "MISC"},
                    new FakeAttribute{Tag = "XREF", Value = "", Layer = "XREF"},
                    new FakeAttribute{Tag = "DESC1", Value = freeLinkNumber.ToString(), Layer = "DESC"},
                    new FakeAttribute{Tag = "WIRENO", Value = "", Layer = "WIREREF"}
                };

                var sourceBlockName = LinkerHelper.GetBlockNameByWireDirectionSource(fullWire.SourceWire);
                                
                BlockUtils.InsertBlockFormFile(_db, sourceBlockName, fullWire.SourceWire.PointConnectedToMultiWire, attributes, "SYMS");

                var destinationBlockName = LinkerHelper.GetBlockNameByWireDirectionDestination(fullWire.DestinationWire);
                                
                BlockUtils.InsertBlockFormFile(_db, destinationBlockName, fullWire.DestinationWire.PointConnectedToMultiWire, attributes, "SYMS");
            }
        }

        private void TieWirePairs()
        {
            foreach (var wire in _sourceDestPairIds)
            {
                if (!TieWirePair(wire.Item1, wire.Item2))
                    return;
            }
        }

        private bool TieWirePair(ObjectId sourceObjectId, ObjectId destinationObjectId)
        {
            var polylines = GetAllPolylinesFromSheet();
            try
            {
                var sourceWireEntity = GetEntity(sourceObjectId);
                var destinationWireEntity = GetEntity(destinationObjectId);
                connectedMultiwire = LinkerHelper.GetConnectedMultiwire(sourceWireEntity, destinationWireEntity, polylines);
                if (connectedMultiwire == null)
                {
                    _ed.WriteMessage("Can't find common multiwire");
                    return false;
                }

                _fullWires.Add(new FullWire()
                {
                    SourceWire = LinkerHelper.GetWireConnectedToMultiWire(_db, connectedMultiwire, sourceWireEntity),
                    DestinationWire = LinkerHelper.GetWireConnectedToMultiWire(_db, connectedMultiwire, destinationWireEntity)
                });
                return true;
            }
            catch
            {
                return false;
            }
        }

        private IEnumerable<Polyline> GetAllPolylinesFromSheet()
        {
            var wires = LinkerHelper.SelectAllPolylineByLayer(_ed, _multiWireLayer).ToList();

            using var acTrans = _db.TransactionManager.StartTransaction();
            foreach (ObjectId wireId in wires)
            {
                yield return (Polyline)acTrans.GetObject(wireId, OpenMode.ForRead);
            }
        }

        private Entity GetEntity(ObjectId objectId)
        {
            //using var acTrans = _db.TransactionManager.StartTransaction();            
            //return (Entity)acTrans.GetObject(objectId, OpenMode.ForRead);
            return (Entity)objectId.GetObject(OpenMode.ForRead, false);
        }

        private IEnumerable<int> ConvertStringsToInts(IEnumerable<string> strings)
        {
            foreach(var str in strings)
            {
                if(int.TryParse(str, out var intNumber))
                    yield return intNumber;
            }
        }

        private int GetFreeNumb(IEnumerable<int> numbers)
        {
            if(numbers == null && numbers.Count() < 1) return 0;

            for(var i = 0; i < numbers.Count(); i++)
            {
                var desc1val = i + 1;
                if (numbers.ElementAt(i) != desc1val)
                    return desc1val;
            }
            return numbers.Count()+1;
        }
    }
}
