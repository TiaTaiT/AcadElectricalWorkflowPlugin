using AutocadCommands.Helpers;
using AutocadCommands.Services;
using CommonHelpers;
using CommonHelpers.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CommonHelpers.Models.IAutocadDirectionEnum;
using Exception = System.Exception;


namespace LinkCommands.Models
{
    public class HalfWire
    {
        private readonly Database _db;
        private readonly Transaction _tr;
        private readonly Direction _direction;

        private string GetShortAttributeMultiwireConnected(string blockName)
        {
            if (blockName.Equals(LinkStruct.Left.SourceBlockName) || blockName.Equals(LinkStruct.Left.DestinationBlockName))
                return LinkStruct.Left.AttributeMultiwire;
            if (blockName.Equals(LinkStruct.Down.SourceBlockName) || blockName.Equals(LinkStruct.Down.DestinationBlockName))
                return LinkStruct.Down.AttributeMultiwire;
            if (blockName.Equals(LinkStruct.Right.SourceBlockName) || blockName.Equals(LinkStruct.Right.DestinationBlockName))
                return LinkStruct.Right.AttributeMultiwire;
            if (blockName.Equals(LinkStruct.Up.SourceBlockName) || blockName.Equals(LinkStruct.Up.DestinationBlockName))
                return LinkStruct.Up.AttributeMultiwire;

            throw new Exception("GetAttributeMultiwireConnected. Block name not found!");
        }

        private string GetShortAttributeWireConnected(string blockName)
        {
            if (blockName.Equals(LinkStruct.Left.SourceBlockName) || blockName.Equals(LinkStruct.Left.DestinationBlockName))
                return LinkStruct.Left.AttributeWire;
            if (blockName.Equals(LinkStruct.Down.SourceBlockName) || blockName.Equals(LinkStruct.Down.DestinationBlockName))
                return LinkStruct.Down.AttributeWire;
            if (blockName.Equals(LinkStruct.Right.SourceBlockName) || blockName.Equals(LinkStruct.Right.DestinationBlockName))
                return LinkStruct.Right.AttributeWire;
            if (blockName.Equals(LinkStruct.Up.SourceBlockName) || blockName.Equals(LinkStruct.Up.DestinationBlockName))
                return LinkStruct.Up.AttributeWire;

            throw new Exception("GetAttributeMultiwireConnected. Block name not found!");
        }

        private Point3d GetMultiWirePoint(ObjectId LinkSimbolId)
        {
            var blockRef = (BlockReference)_tr.GetObject(LinkSimbolId, OpenMode.ForRead);
            var attributes = blockRef.AttributeCollection;
            foreach (ObjectId attributeId in attributes)
            {
                AttributeReference attRef = (AttributeReference)_tr.GetObject(attributeId, OpenMode.ForWrite);
                var tagStartSymbols = GetShortAttributeMultiwireConnected(blockRef.Name);
                if (attRef.Tag.ToUpper().StartsWith(tagStartSymbols))
                {
                    return attRef.Position;
                }
            }
            throw new Exception("Multiwire attribute in symbol not found!");
        }

        private Point3d GetZeroPoint(Entity sourceEntity)
        {
            if (sourceEntity is Polyline polyline)
            {
                if (polyline.EndPoint.IsEqualTo(PointConnectedToMultiWire))
                    return polyline.EndPoint;
                return polyline.StartPoint;
            }
            if (sourceEntity is Line line)
            {
                if (line.EndPoint.IsEqualTo(PointConnectedToMultiWire))
                    return line.EndPoint;
                return line.StartPoint;
            }
            throw new ArgumentException("Source or destination wire aren't line or polyline type");
        }

        private Point3d GetEndPoint(Entity sourceEntity)
        {
            if (sourceEntity is Polyline polyline)
            {
                if (!polyline.EndPoint.IsEqualTo(PointConnectedToMultiWire))
                    return polyline.EndPoint;
                return polyline.StartPoint;
            }
            if (sourceEntity is Line line)
            {
                if (!line.EndPoint.IsEqualTo(PointConnectedToMultiWire))
                    return line.EndPoint;
                return line.StartPoint;
            }
            throw new ArgumentException("Source or destination wire aren't line or polyline type");
        }

        private List<FakeAttribute> GetAttributes()
        {
            return
            [
                new() {Tag = "X8_TINY_DOT_DONT_REMOVE", Layer = "MISC"},
                new() {Tag = "X2_TINY_DOT_DONT_REMOVE", Layer = "MISC"},
                new() {Tag = "X2TERM02", Layer = "MISC"},
                new() {Tag = "X2TERM01", Layer = "MISC"},
                new() {Tag = "SIGCODE", Value = SigCode, Layer = "MISC"},
                new() {Tag = "XREF", Value = "", Layer = "XREF"},
                new() {Tag = "DESC1", Value = ShortDescription, Layer = "DESC"},
                new() {Tag = "WIRENO", Value = "", Layer = "WIREREF"}
            ];
        }

        private string GetSourceSymbolBlockName()
        {
            return _direction switch
            {
                Direction.Left => LinkStruct.Left.SourceBlockName,
                Direction.Below => LinkStruct.Down.SourceBlockName,
                Direction.Right => LinkStruct.Right.SourceBlockName,
                Direction.Above => LinkStruct.Up.SourceBlockName,
                _ => throw new Exception("Unknown direction!"),
            };
        }

        private string GetDestinationSymbolBlockName()
        {
            return _direction switch
            {
                Direction.Left => LinkStruct.Left.DestinationBlockName,
                Direction.Below => LinkStruct.Down.DestinationBlockName,
                Direction.Right => LinkStruct.Right.DestinationBlockName,
                Direction.Above => LinkStruct.Up.DestinationBlockName,
                _ => throw new Exception("Unknown direction!"),
            };
        }

        private bool TryGetExistingLinkSymbol(Entity wire, out ObjectId output)
        {
            var strings = new List<string>()
            {
                LinkStruct.Left.SourceBlockName,
                LinkStruct.Left.DestinationBlockName,
                LinkStruct.Right.SourceBlockName,
                LinkStruct.Right.DestinationBlockName,
                LinkStruct.Down.SourceBlockName,
                LinkStruct.Down.DestinationBlockName,
                LinkStruct.Up.SourceBlockName,
                LinkStruct.Up.DestinationBlockName
            };
            var blockRefIds = GetObjectsUtils.GetBlockIdsByNames(_db, _tr, strings);

            foreach (var blkRefId in blockRefIds)
            {
                var attributesCollection = ((BlockReference)(blkRefId.GetObject(OpenMode.ForRead, false))).AttributeCollection;

                foreach (ObjectId attrId in attributesCollection)
                {
                    var att = (AttributeReference)(attrId.GetObject(OpenMode.ForRead, false));
                    if (att.Position.Equals(GetZeroPoint(wire)))
                    {
                        output = blkRefId;
                        return true;
                    }
                }
            }
            output = new ObjectId();
            return false;
        }

        private void CreateSymbol(Database db, string symbolBlockName)
        {
            var attributes = GetAttributes();

            var success = TryGetExistingLinkSymbol(WireEntity, out var existingLinkSymbolId);
            if (success)
            {
                var result = AttributeHelper.SetBlockFakeAttributes(_tr, existingLinkSymbolId, attributes);
                //Debug.WriteLine("Ids = " + existingLinkSymbolId.ToString());
            }
            else
            {
                var linkSymbolId = BlockUtils.InsertBlockFormFile(db, symbolBlockName, PointConnectedToMultiWire, attributes, Layers.Symbols);
                var originSymbolPoint = ((BlockReference)linkSymbolId.GetObject(OpenMode.ForRead)).Position;
                var shiftPoint = GetMultiWirePoint(linkSymbolId);
                //Debug.WriteLine("Original X: " + originSymbolPoint.X + " -> " + shiftPoint.X);
                //Debug.WriteLine("Original Y: " + originSymbolPoint.Y + " -> " + shiftPoint.Y);
                //Debug.WriteLine("Original Z: " + originSymbolPoint.Z + " -> " + shiftPoint.Z);
                var moveVec = new Vector3d(originSymbolPoint.X - shiftPoint.X, originSymbolPoint.Y - shiftPoint.Y, originSymbolPoint.Z - shiftPoint.Z);
                BlockUtils.MoveBlockReference(_db, linkSymbolId, moveVec);
            }
        }

        private IEnumerable<ElectricalComponent> _components;

        public Entity WireEntity { get; set; }

        public Point3d PointConnectedToMultiWire { get; set; }

        public HalfWire(Transaction tr, IEnumerable<Curve> curves, Link link)
        {
            _tr = tr;
            Curves = curves;
            PointLinkAttachedToWire = link.WireConnectionPoint;
            Description = link.Description;
            SigCode = link.SigCode;
            LinkSymbol = link.Reference;
        }

        public HalfWire(Transaction tr, Entity wireEntity, IEnumerable<ElectricalComponent> components)
        {
            _tr = tr;
            if (wireEntity == null)
                return;

            _components = components;

            WireEntity = wireEntity;

            _db = Application.DocumentManager.MdiActiveDocument.Database;
            Tolerance.Global = new Tolerance(1e-8, 1e-1);

            var wireEntities = GeometryFunc.GetAllConjugatedCurves(_db, _tr, (Curve)wireEntity, Layers.Wires);

            SetWireMultiwireCross((Curve)WireEntity);

            foreach (var entity in wireEntities.Cast<Curve>())
            {
                SetWireComponentCross(entity);
            }

            var zeroPoint = GetZeroPoint(WireEntity);

            _direction = GeometryFunc.GetDirection(zeroPoint, GetEndPoint(WireEntity));

        }


        private void SetWireComponentCross(Curve curve)
        {
            Parallel.ForEach(_components, new ParallelOptions() { MaxDegreeOfParallelism = 8 },
                (item, i, j) => FindWireTerminalCross(item, i, curve));
        }

        private void FindWireTerminalCross(ElectricalComponent component, ParallelLoopState loopState, Curve curve)
        {
            foreach (var terminal in component.Terminals)
            {
                //Debug.WriteLineIf(component.IsTerminal, component.Name + "; Terminals.count: " + component.Terminals.Count() + ". Connection points count = " + terminal.Points.Count());
                if (terminal.IsContainPoint(curve.StartPoint))
                {
                    SetWireDescription(curve.StartPoint, component, terminal);
                    loopState.Stop();

                }
                if (terminal.IsContainPoint(curve.EndPoint))
                {
                    SetWireDescription(curve.EndPoint, component, terminal);
                    loopState.Stop();
                }
            }
        }

        private void SetWireDescription(Point3d point, ElectricalComponent component, ComponentTerminal terminal)
        {
            PointConnectedToComponent = point;
            Terminal = terminal;

            // Input/Output terminals have name field as the Description
            if (component.IsTerminal)
            {
                Description = component.Name;
                return;
            }
            // Rest components have description in the Value field
            Description = terminal.Value;
        }

        public void SetWireMultiwireCross(Curve wireEntity)
        {
            var multiWires = new List<Curve>();
            multiWires.AddRange(GetObjectsUtils.GetObjects<Polyline>(_db, _tr, Layers.MultiWires));

            multiWires.AddRange(GetObjectsUtils.GetObjects<Line>(_db, _tr, Layers.MultiWires));

            if (multiWires == null || multiWires.Count() == 0)
            {
                return;
            }

            foreach (var multiWire in multiWires)
            {
                var polyWire = (Curve)multiWire;

                var result = LinkerHelper.TryGetPointConnectedToMultiwire(polyWire, wireEntity, out var crossPoint);
                if (result)
                {
                    PointConnectedToMultiWire = crossPoint;
                    return;
                }
            }
        }

        public void CreateSourceLink()
        {
            string symbolBlockName = GetSourceSymbolBlockName();
            CreateSymbol(_db, symbolBlockName);
        }

        public void CreateDestinationLink()
        {
            string symbolBlockName = GetDestinationSymbolBlockName();
            CreateSymbol(_db, symbolBlockName);
        }

        /// <summary>
        /// Attribute carries the unique signal code that is user-defined as the symbol is inserted.
        /// This value is used to match up each source signal with its destination signals.
        /// </summary>
        public string SigCode { get; set; }

        /// <summary>
        /// Description attribute
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Attached terminal
        /// </summary>
        public ComponentTerminal Terminal { get; set; }

        // Source/Destination Wire Signal Symbol
        public BlockReference LinkSymbol { get; set; }
        public Point3d PointConnectedToComponent { get; private set; }

        public Point3d PointLinkAttachedToWire { get; private set; }

        public string ShortDescription { get; set; }

        public IEnumerable<Curve> Curves { get; private set; }

        public bool IsSource
        {
            get
            {
                if ((LinkSymbol.BlockName[3] == 'S') || (LinkSymbol.BlockName[3] == 's'))
                    return true;
                return false;
            }
            private set { }
        }

        public void Clean()
        {
            LinkSymbol.Erase();
        }

        public bool IsPointOnEnd(Point3d point)
        {
            return GeometryFunc.IsPointOnCurveEnd(point, Curves);
        }
    }
}
