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
        private List<Entity> _sourceMultiwireEntities = new();
        private List<Entity> _destinationMultiwireEntities = new();

        private Entity _selectedSourceLine;
        private Entity _selectedDestinationLine;

        private List<string> _existedSigCodes = new();
        
        private List<ObjectId> _sourceLinkSymbols = new();
        private List<ObjectId> _destinationLinkSymbols = new();
        private List<(ObjectId, ObjectId)> _sourceDestinationLinkSymbols = new();

        private char _up = '4';
        private char _down = '2';
        private char _right = '3';
        private char _left = '1';

        private string _symbolPrefix = "HA";
        private string _symbolTypeWave = "4";
        private string _symbolTypeHexagon = "3";
        private string _sourceSymbolCode = "S";
        private string _destinationSymbolCode = "D";
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
                _selectedSourceLine = (Entity)selectionBlock1.ObjectId.GetObject(OpenMode.ForRead);
                _sourceMultiwireEntities = GeometryFunc.GetAllConjugatedEntities(_db, selectionBlock1.ObjectId, Layers.MultiWires).ToList();
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
                _selectedDestinationLine = (Entity)selectionBlock2.ObjectId.GetObject(OpenMode.ForRead);
                _destinationMultiwireEntities = GeometryFunc.GetAllConjugatedEntities(_db, selectionBlock2.ObjectId, Layers.MultiWires).ToList();
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

            var sourceSymbolName = GetSourceName(sourceDirection, _symbolTypeWave);
            var destinationSymbolName = GetDestinationName(destinationDirection, _symbolTypeWave);

            var description = GetFirstFreeNumber(tr).ToString();
            var newSigCode = Guid.NewGuid().ToString();

            var attributes = GetAttributes(description, newSigCode);

            _sourceLinkSymbolId = BlockUtils.InsertBlockFormFile(_db, sourceSymbolName, sourceSymbolPoint, attributes, Layers.Symbols);
            _destinationLinkSymbolId = BlockUtils.InsertBlockFormFile(_db, destinationSymbolName, destinationSymbolPoint, attributes, Layers.Symbols);
        }

        private string GetDestinationName(IAutocadDirectionEnum.Direction destinationDirection, string symbolTypeWave)
        {
            return destinationDirection switch
            {
                IAutocadDirectionEnum.Direction.Right => _symbolPrefix + symbolTypeWave + _destinationSymbolCode + _right,
                IAutocadDirectionEnum.Direction.Above => _symbolPrefix + symbolTypeWave + _destinationSymbolCode + _up,
                IAutocadDirectionEnum.Direction.Left => _symbolPrefix + symbolTypeWave + _destinationSymbolCode + _left,
                IAutocadDirectionEnum.Direction.Below => _symbolPrefix + symbolTypeWave + _destinationSymbolCode + _down,
                _ => _symbolPrefix + symbolTypeWave + _destinationSymbolCode + _up,
            };
        }

        private string GetSourceName(IAutocadDirectionEnum.Direction sourceDirection, string symbolTypeWave)
        {
            return sourceDirection switch
            {
                IAutocadDirectionEnum.Direction.Right => _symbolPrefix + symbolTypeWave + _sourceSymbolCode + _right,
                IAutocadDirectionEnum.Direction.Above => _symbolPrefix + symbolTypeWave + _sourceSymbolCode + _up,
                IAutocadDirectionEnum.Direction.Left => _symbolPrefix + symbolTypeWave + _sourceSymbolCode + _left,
                IAutocadDirectionEnum.Direction.Below => _symbolPrefix + symbolTypeWave + _sourceSymbolCode + _down,
                _ => _symbolPrefix + symbolTypeWave + _sourceSymbolCode + _down,
            };
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

        private (Point3d, Point3d) GetZeroEndPoints(Transaction acTrans, Entity selectedMultiwirePart)
        {
            var startPoint = ((Curve)selectedMultiwirePart).StartPoint;
            var endPoint = ((Curve)selectedMultiwirePart).EndPoint;

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
                var wireIds = LinkerHelper.GetAllWireIdsFromDb(_db);
                foreach (var id in wireIds)
                {
                    var curveWire = (Curve)id.GetObject(OpenMode.ForRead);

                    if (curveWire.StartPoint.Equals(((Curve)selectedMultiwirePart).StartPoint) ||
                        curveWire.EndPoint.Equals(((Curve)selectedMultiwirePart).StartPoint))
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

        private void GetExistSourceDestinationSymbols()
        {
            var sourceNames = new List<string>
            {
                _symbolPrefix + _symbolTypeWave + _sourceSymbolCode + _up,
                _symbolPrefix + _symbolTypeWave + _sourceSymbolCode + _left,
                _symbolPrefix + _symbolTypeWave + _sourceSymbolCode + _down,
                _symbolPrefix + _symbolTypeWave + _sourceSymbolCode + _right
            };
            _sourceLinkSymbols.AddRange(GetIdsUtils.GetBlockRefsByNames(_db, sourceNames));

            var destinationNames = new List<string>
            {
                _symbolPrefix + _symbolTypeWave + _destinationSymbolCode + _up,
                _symbolPrefix + _symbolTypeWave + _destinationSymbolCode + _left,
                _symbolPrefix + _symbolTypeWave + _destinationSymbolCode + _down,
                _symbolPrefix + _symbolTypeWave + _destinationSymbolCode + _right
            };
            _destinationLinkSymbols.AddRange(GetIdsUtils.GetBlockRefsByNames(_db, destinationNames));
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
