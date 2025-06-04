using AutocadCommands.Helpers;
using AutocadCommands.Models;
using AutocadCommands.Services;
using CommonHelpers;
using CommonHelpers.Model;
using LinkCommands.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkCommands.Services
{
    internal class PairMultiWiresLinker(Document doc) : CommandPrototype(doc)
    {
        private List<Curve> _sourceMultiwireEntities = [];
        private List<Curve> _destinationMultiwireEntities = [];

        private Curve _selectedSourceLine;
        private Curve _selectedDestinationLine;

        private readonly List<string> _existedSigCodes = [];

        private readonly List<ObjectId> _sourceLinkSymbols = [];
        private readonly List<ObjectId> _destinationLinkSymbols = [];
        private readonly List<(ObjectId, ObjectId)> _sourceDestinationLinkSymbols = [];


        private ObjectId _sourceLinkSymbolId;
        private ObjectId _destinationLinkSymbolId;
        private ComponentsFactory _componentsFactory;
        private NetsFactory _netsFactory;

        private List<FakeAttribute> GetAttributes(string description, string sigCode)
        {
            return
            [
                new() {Tag = "X8_TINY_DOT_DONT_REMOVE", Layer = "MISC"},
                new() {Tag = "X2_TINY_DOT_DONT_REMOVE", Layer = "MISC"},
                new() {Tag = "X2TERM02", Layer = "MISC"},
                new() {Tag = "X2TERM01", Layer = "MISC"},
                new() {Tag = "SIGCODE", Value = sigCode, Layer = "MISC"},
                new() {Tag = "XREF", Value = "", Layer = "XREF"},
                new() {Tag = "DESC1", Value = description, Layer = "DESC"},
                new() {Tag = "WIRENO", Value = "", Layer = "WIREREF"}
            ];
        }

        public override bool Init()
        {
            CreateComponentsFactory();

            //Make the selection
            var options1 = new PromptEntityOptions("\nSelect source line: ");
            options1.SetRejectMessage("\nSelected object is no a line.");
            options1.AddAllowedClass(typeof(Line), true);

            var selectionBlock1 = _ed.GetEntity(options1);
            if (selectionBlock1.Status != PromptStatus.OK)
                return false;

            if (selectionBlock1.ObjectId == null)
                return false;


            _selectedSourceLine = (Curve)selectionBlock1.ObjectId.GetObject(OpenMode.ForRead);
            _sourceMultiwireEntities = GeometryFunc.GetAllConjugatedCurves(_db, _tr, _selectedSourceLine, Layers.MultiWires).ToList();
            HighlightObjects(_sourceMultiwireEntities);


            var options2 = new PromptEntityOptions("\nSelect destination line: ");
            options2.SetRejectMessage("\nSelected object is no a line.");
            options2.AddAllowedClass(typeof(Line), true);

            var selectionBlock2 = _ed.GetEntity(options2);
            if (selectionBlock2.Status != PromptStatus.OK)
                return false;

            if (selectionBlock2.ObjectId == null)
                return false;

            _selectedDestinationLine = (Curve)selectionBlock2.ObjectId.GetObject(OpenMode.ForRead);
            _destinationMultiwireEntities = GeometryFunc.GetAllConjugatedCurves(_db, _tr, _selectedDestinationLine, Layers.MultiWires).ToList();
            HighlightObjects(_destinationMultiwireEntities);

            return _sourceMultiwireEntities.Any() && _destinationMultiwireEntities.Any();
        }

        public override void Run()
        {
            _db.TransactionManager.QueueForGraphicsFlush();

            GetExistSourceDestinationSymbols();

            var linkEndSourcePoints = GetZeroEndPoints(_selectedSourceLine);
            var sourceSymbolPoint = linkEndSourcePoints.Item1;
            var sourceEndPoint = linkEndSourcePoints.Item2;

            var linkEndDestinationPoints = GetZeroEndPoints(_selectedDestinationLine);
            var destinationSymbolPoint = linkEndDestinationPoints.Item1;
            var destinationEndPoint = linkEndDestinationPoints.Item2;

            _ed.WriteMessage("Remove links count: " + _existedSigCodes.Count());

            var erasedSymbols = EraseUnpairSymbols().ToList();

            erasedSymbols.AddRange(BlockHelper.EraseBlockIfExist(_db, _tr, sourceSymbolPoint));

            erasedSymbols.AddRange(BlockHelper.EraseBlockIfExist(_db, _tr, destinationSymbolPoint));

            RemoveSourceDestination(erasedSymbols);

            InsertSourceDestinationLinkSymbols(sourceSymbolPoint, sourceEndPoint, destinationSymbolPoint, destinationEndPoint);

            GenerateMultiwire();

            UnhighlightObjects(_sourceMultiwireEntities);
            UnhighlightObjects(_destinationMultiwireEntities);
        }

        private void CreateComponentsFactory()
        {
            _componentsFactory = new ComponentsFactory(_db, _tr);
            _netsFactory = new NetsFactory(_db, _tr, _componentsFactory.GetTerminalPoints());
            var terminals = _componentsFactory.GetAllTerminalsInComponents();
            ComponentsWiresTier.CreateElectricalNet(terminals, _netsFactory.Wires);
        }

        private void GetExistSourceDestinationSymbols()
        {
            var sourceNames = LinkSymbolNameResolver.GetSourceSymbolNames();
            _sourceLinkSymbols.AddRange(GetObjectsUtils.GetBlockIdsByNames(_db, _tr, sourceNames));

            var destinationNames = LinkSymbolNameResolver.GetSourceSymbolNames();
            _destinationLinkSymbols.AddRange(GetObjectsUtils.GetBlockIdsByNames(_db, _tr, destinationNames));
        }

        private void GenerateMultiwire()
        {
            var sourceSymbolEntity = (Entity)_sourceLinkSymbolId.GetObject(OpenMode.ForRead);
            var destinationSymbolEntity = (Entity)_destinationLinkSymbolId.GetObject(OpenMode.ForRead);

            var multiwire =
                new MultiWire(_db,
                              _tr,
                              _sourceMultiwireEntities,
                              sourceSymbolEntity,
                              _destinationMultiwireEntities,
                              destinationSymbolEntity,
                              _componentsFactory.Components);
            multiwire.Create();
        }

        private void InsertSourceDestinationLinkSymbols(Point3d sourceSymbolPoint, Point3d sourceEndPoint, Point3d destinationSymbolPoint, Point3d destinationEndPoint)
        {
            var sourceDirection = GeometryFunc.GetDirection(sourceSymbolPoint, sourceEndPoint);
            var destinationDirection = GeometryFunc.GetDirection(destinationSymbolPoint, destinationEndPoint);

            var sourceSymbolName = LinkSymbolNameResolver.GetSourceName(sourceDirection,
                LinkSymbolNameResolver._symbolTypeWave);
            var destinationSymbolName = LinkSymbolNameResolver.GetDestinationName(destinationDirection,
                LinkSymbolNameResolver._symbolTypeWave);

            var description = GetFirstFreeNumber().ToString();
            var newSigCode = Guid.NewGuid().ToString();

            var attributes = GetAttributes(description, newSigCode);

            _sourceLinkSymbolId = BlockUtils.InsertBlockFormFile(_db, sourceSymbolName, sourceSymbolPoint, attributes, Layers.Symbols);
            _destinationLinkSymbolId = BlockUtils.InsertBlockFormFile(_db, destinationSymbolName, destinationSymbolPoint, attributes, Layers.Symbols);
        }



        private IEnumerable<ObjectId> EraseUnpairSymbols()
        {
            // We must not leave unpaired link symbols
            var erasedList = new List<ObjectId>();
            foreach (var sigCode in _existedSigCodes)
            {
                erasedList.AddRange(BlockHelper.EraseBlocksWithAttribute(_db, _tr, "SIGCODE", sigCode));
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

        private (Point3d, Point3d) GetZeroEndPoints(Curve selectedMultiwirePart)
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
                        _existedSigCodes.Add(AttributeHelper.GetAttributeValueFromBlock(_tr, symbolId, "SIGCODE"));
                        return (startPoint, endPoint);
                    }
                    if (symbolBlkRef.Position.Equals(endPoint))
                    {
                        _existedSigCodes.Add(AttributeHelper.GetAttributeValueFromBlock(_tr, symbolId, "SIGCODE"));
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
                var wires = LinkerHelper.GetAllWiresFromDb(_db, _tr);
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

            if (_sourceMultiwireEntities.Contains(selectedMultiwirePart))
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
            foreach (var entity in entities)
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

        private int GetFirstFreeNumber()
        {
            var existedDescriptions = new SortedSet<int>();
            foreach (var symbolId in _sourceLinkSymbols)
            {
                var description = AttributeHelper.GetAttributeValueFromBlock(_tr, symbolId, "DESC1");

                if (int.TryParse(description, out var number))
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
