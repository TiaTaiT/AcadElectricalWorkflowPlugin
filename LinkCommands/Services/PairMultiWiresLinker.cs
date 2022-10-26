using AutocadCommands.Helpers;
using AutocadCommands.Models;
using AutocadCommands.Services;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows.BlockStream;
using CommonHelpers;
using CommonHelpers.Model;
using CommonHelpers.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkCommands.Services
{
    internal class PairMultiWiresLinker : CommandPrototype
    {
        private List<Curve> _sourceMultiwireEntities = new();
        private List<Curve> _destinationMultiwireEntities = new();

        private Curve _selectedSourceLine;
        private Curve _selectedDestinationLine;

        private List<string> _existedSigCodes = new();
        
        private List<ObjectId> _sourceLinkSymbols = new();
        private List<ObjectId> _destinationLinkSymbols = new();
        private List<(ObjectId, ObjectId)> _sourceDestinationLinkSymbols = new();

        
        private ObjectId _sourceLinkSymbolId;
        private ObjectId _destinationLinkSymbolId;

        private List<FakeAttribute> GetAttributes(string description, string sigCode)
        {
            return new List<FakeAttribute>
            {
                new FakeAttribute{Tag = "X8_TINY_DOT_DONT_REMOVE", Layer = "MISC"},
                new FakeAttribute{Tag = "X2_TINY_DOT_DONT_REMOVE", Layer = "MISC"},
                new FakeAttribute{Tag = "X2TERM02", Layer = "MISC"},
                new FakeAttribute{Tag = "X2TERM01", Layer = "MISC"},
                new FakeAttribute{Tag = "SIGCODE", Value = sigCode, Layer = "MISC"},
                new FakeAttribute{Tag = "XREF", Value = "", Layer = "XREF"},
                new FakeAttribute{Tag = "DESC1", Value = description, Layer = "DESC"},
                new FakeAttribute{Tag = "WIRENO", Value = "", Layer = "WIREREF"}
            };
        }

        public PairMultiWiresLinker(Document doc) : base(doc)
        {
        }

        public override bool Init()
        {
            //Make the selection
            var options1 = new PromptEntityOptions("\nSelect source line: ");
            options1.SetRejectMessage("\nSelected object is no a line.");
            options1.AddAllowedClass(typeof(Line), true);

            var selectionBlock1 = _ed.GetEntity(options1);
            if (selectionBlock1.Status != PromptStatus.OK)
                return false;

            if (selectionBlock1.ObjectId == null)
                return false;

            using (var tr = _db.TransactionManager.StartTransaction())
            {
                _selectedSourceLine = (Curve)selectionBlock1.ObjectId.GetObject(OpenMode.ForRead);
                _sourceMultiwireEntities = GeometryFunc.GetAllConjugatedCurves(_db, _selectedSourceLine, Layers.MultiWires).ToList();
                HighlightObjects(_sourceMultiwireEntities);
            }

            var options2 = new PromptEntityOptions("\nSelect destination line: ");
            options2.SetRejectMessage("\nSelected object is no a line.");
            options2.AddAllowedClass(typeof(Line), true);

            var selectionBlock2 = _ed.GetEntity(options2);
            if (selectionBlock2.Status != PromptStatus.OK)
                return false;

            if(selectionBlock2.ObjectId == null)
                return false;

            using (var tr = _db.TransactionManager.StartTransaction())
            {
                _selectedDestinationLine = (Curve)selectionBlock2.ObjectId.GetObject(OpenMode.ForRead);
                _destinationMultiwireEntities = GeometryFunc.GetAllConjugatedCurves(_db, _selectedDestinationLine, Layers.MultiWires).ToList();
                HighlightObjects(_destinationMultiwireEntities);
            }

            return _sourceMultiwireEntities.Any() && _destinationMultiwireEntities.Any();
        }

        public override void Run()
        {
            using var tr = _db.TransactionManager.StartTransaction();

            _db.TransactionManager.QueueForGraphicsFlush();

            GetExistSourceDestinationSymbols();

            var linkEndSourcePoints = GetZeroEndPoints(tr, _selectedSourceLine);
            var sourceSymbolPoint = linkEndSourcePoints.Item1;
            var sourceEndPoint = linkEndSourcePoints.Item2;

            var linkEndDestinationPoints = GetZeroEndPoints(tr, _selectedDestinationLine);
            var destinationSymbolPoint = linkEndDestinationPoints.Item1;
            var destinationEndPoint = linkEndDestinationPoints.Item2;

            _ed.WriteMessage("Remove links count: " + _existedSigCodes.Count());

            var erasedSymbols = EraseUnpairSymbols(tr).ToList();

            erasedSymbols.AddRange(BlockHelper.EraseBlockIfExist(_db, tr, sourceSymbolPoint));

            erasedSymbols.AddRange(BlockHelper.EraseBlockIfExist(_db, tr, destinationSymbolPoint));

            RemoveSourceDestination(erasedSymbols);

            InsertSourceDestinationLinkSymbols(tr, sourceSymbolPoint, sourceEndPoint, destinationSymbolPoint, destinationEndPoint);

            GenerateMultiwire();

            UnhighlightObjects(_sourceMultiwireEntities);
            UnhighlightObjects(_destinationMultiwireEntities);

            tr.Commit();
        }

        private void GetExistSourceDestinationSymbols()
        {
            var sourceNames = LinkSymbolNameResolver.GetSourceSymbolNames();
            _sourceLinkSymbols.AddRange(GetObjectsUtils.GetBlockIdsByNames(_db, sourceNames));

            var destinationNames = LinkSymbolNameResolver.GetSourceSymbolNames();
            _destinationLinkSymbols.AddRange(GetObjectsUtils.GetBlockIdsByNames(_db, destinationNames));
        }

        private void GenerateMultiwire()
        {
            var sourceSymbolEntity = (Entity)_sourceLinkSymbolId.GetObject(OpenMode.ForRead);
            var destinationSymbolEntity = (Entity)_destinationLinkSymbolId.GetObject(OpenMode.ForRead);

            var multiwire = new MultiWire(_sourceMultiwireEntities, sourceSymbolEntity, _destinationMultiwireEntities, destinationSymbolEntity);
            multiwire.Create();
        }

        private void InsertSourceDestinationLinkSymbols(Transaction tr, Point3d sourceSymbolPoint, Point3d sourceEndPoint, Point3d destinationSymbolPoint, Point3d destinationEndPoint)
        {
            var sourceDirection = GeometryFunc.GetDirection(sourceSymbolPoint, sourceEndPoint);
            var destinationDirection = GeometryFunc.GetDirection(destinationSymbolPoint, destinationEndPoint);

            var sourceSymbolName = LinkSymbolNameResolver.GetSourceName(sourceDirection,
                LinkSymbolNameResolver._symbolTypeWave);
            var destinationSymbolName = LinkSymbolNameResolver.GetDestinationName(destinationDirection,
                LinkSymbolNameResolver._symbolTypeWave);

            var description = GetFirstFreeNumber(tr).ToString();
            var newSigCode = Guid.NewGuid().ToString();

            var attributes = GetAttributes(description, newSigCode);

            _sourceLinkSymbolId = BlockUtils.InsertBlockFormFile(_db, sourceSymbolName, sourceSymbolPoint, attributes, Layers.Symbols);
            _destinationLinkSymbolId = BlockUtils.InsertBlockFormFile(_db, destinationSymbolName, destinationSymbolPoint, attributes, Layers.Symbols);
        }

        

        private IEnumerable<ObjectId> EraseUnpairSymbols(Transaction tr)
        {
            // We must not leave unpaired link symbols
            var erasedList = new List<ObjectId>();
            foreach (var sigCode in _existedSigCodes)
            {
                erasedList.AddRange(BlockHelper.EraseBlocksWithAttribute(_db, tr, "SIGCODE", sigCode));
            }
            return erasedList;
        }

        private void RemoveSourceDestination(IEnumerable<ObjectId> erasedIds)
        {
            foreach (var id in erasedIds)
            {
                _sourceLinkSymbols.RemoveAll(i => i.Equals(id));
                _destinationLinkSymbols.RemoveAll(i => i.Equals(id));
            }
        }

        private (Point3d, Point3d) GetZeroEndPoints(Transaction acTrans, Curve selectedMultiwirePart)
        {
            var startPoint = selectedMultiwirePart.StartPoint;
            var endPoint = selectedMultiwirePart.EndPoint;

            var existingLinkSymbols = new List<ObjectId>();
            existingLinkSymbols.AddRange(_sourceLinkSymbols);
            existingLinkSymbols.AddRange(_destinationLinkSymbols);

            if (existingLinkSymbols.Count > 0)
            {
                foreach (var symbolId in existingLinkSymbols)
                {
                    var symbolBlkRef = (BlockReference)symbolId.GetObject(OpenMode.ForRead);
                    if (symbolBlkRef.Position.Equals(startPoint))
                    {
                        _existedSigCodes.Add(AttributeHelper.GetAttributeValueFromBlock(acTrans, symbolId, "SIGCODE"));
                        return (startPoint, endPoint);
                    }
                    if (symbolBlkRef.Position.Equals(endPoint))
                    {
                        _existedSigCodes.Add(AttributeHelper.GetAttributeValueFromBlock(acTrans, symbolId, "SIGCODE"));
                        return (endPoint, startPoint);
                    }
                }
            }

            var sourceAndDestinationMultiwires = new List<Entity>();
            sourceAndDestinationMultiwires.AddRange(_sourceMultiwireEntities);
            sourceAndDestinationMultiwires.AddRange(_destinationMultiwireEntities);

            if ((_sourceMultiwireEntities.Count() == 1 && _sourceMultiwireEntities.Contains(selectedMultiwirePart)) 
                || (_destinationMultiwireEntities.Count() == 1 && _destinationMultiwireEntities.Contains(selectedMultiwirePart)))
            {
                var wires = LinkerHelper.GetAllWiresFromDb(_db);
                foreach (var wire in wires)
                {
                    if (wire.StartPoint.Equals(selectedMultiwirePart.StartPoint) ||
                        wire.EndPoint.Equals(selectedMultiwirePart.StartPoint))
                    {
                        return (endPoint, startPoint);
                    }
                }
                return (startPoint, endPoint);
            }

            if(_sourceMultiwireEntities.Contains(selectedMultiwirePart))
            {
                foreach (var entity in _sourceMultiwireEntities)
                {

                    if (entity.ObjectId == selectedMultiwirePart.ObjectId)
                        continue;

                    var line = (Line)entity;
                    if (GeometryFunc.IsPointOnLine(line, startPoint)) //IsPtOnLine(line, startPoint))
                        return (endPoint, startPoint);
                }

                return (startPoint, endPoint);
            }

            foreach (var entity in _destinationMultiwireEntities)
            {

                if (entity.ObjectId == selectedMultiwirePart.ObjectId)
                    continue;

                var line = (Line)entity;
                if (GeometryFunc.IsPointOnLine(line, startPoint))//IsPtOnLine(line, startPoint))
                    return (endPoint, startPoint);
            }

            return (startPoint, endPoint);
        }

        

        private void HighlightObjects(IEnumerable<Entity> entities)
        {
            foreach(var entity in entities)
            {
                entity.Highlight();
            }
        }

        private void UnhighlightObjects(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                entity.Unhighlight();
            }
        }

        private int GetFirstFreeNumber(Transaction tr)
        {
            var existedDescriptions = new SortedSet<int>();
            foreach(var symbolId in _sourceLinkSymbols)
            {
                var description = AttributeHelper.GetAttributeValueFromBlock(tr, symbolId, "DESC1");
                
                if(int.TryParse(description, out var number))
                {
                    existedDescriptions.Add(number);
                }
            }

            int firstFreeAvailable = Enumerable.Range(0, int.MaxValue)
                                .Except(existedDescriptions)
                                .FirstOrDefault();

            return firstFreeAvailable;
        }
    }
}
