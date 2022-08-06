using AutocadCommands.Helpers;
using AutocadCommands.Models;
using AutocadCommands.Services;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using CommonHelpers;
using CommonHelpers.Model;
using LinkCommands.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonHelpers.Models.IAutocadDirectionEnum;
using Exception = System.Exception;

namespace LinkCommands.Models
{
    public class HalfWire
    {
        private Database _db;
        private Direction _direction;

        private readonly struct Left
        {
            public const string SourceBlockName = "ha2s1_inline";
            public const string DestinationBlockName = "ha2d1_inline";
            public const string AttributeMultiwire = "X1TERM";
            public const string AttributeWire = "X4TERM";
        }

        private readonly struct Down
        {
            public const string SourceBlockName = "ha2s2_inline";
            public const string DestinationBlockName = "ha2d2_inline";
            public const string AttributeMultiwire = "X2TERM";
            public const string AttributeWire = "X8TERM";
        }

        private readonly struct Right
        {
            public const string SourceBlockName = "ha2s3_inline";
            public const string DestinationBlockName = "ha2d3_inline";
            public const string AttributeMultiwire = "X4TERM";
            public const string AttributeWire = "X1TERM";
        }

        private readonly struct Up
        {
            public const string SourceBlockName = "ha2s4_inline";
            public const string DestinationBlockName = "ha2d4_inline"; 
            public const string AttributeMultiwire = "X8TERM";
            public const string AttributeWire = "X2TERM";
        }

        private readonly struct Circle
        {
            public const string AttributeWire = "X0TERM";
        }

        private bool IsOneOfTerminalLinks(string str)
        {
            if(string.IsNullOrEmpty(str)) return false;

            if (str.StartsWith(Left.AttributeWire) ||
               str.StartsWith(Right.AttributeWire) ||
               str.StartsWith(Up.AttributeWire) ||
               str.StartsWith(Down.AttributeWire))
                return true;

            return false;
        }

        private string GetShortAttributeMultiwireConnected(string blockName)
        {
            if (blockName.Equals(Left.SourceBlockName) || blockName.Equals(Left.DestinationBlockName))
                return Left.AttributeMultiwire;
            if (blockName.Equals(Down.SourceBlockName) || blockName.Equals(Down.DestinationBlockName))
                return Down.AttributeMultiwire;
            if (blockName.Equals(Right.SourceBlockName) || blockName.Equals(Right.DestinationBlockName))
                return Right.AttributeMultiwire;
            if (blockName.Equals(Up.SourceBlockName) || blockName.Equals(Up.DestinationBlockName))
                return Up.AttributeMultiwire;

            throw new Exception("GetAttributeMultiwireConnected. Block name not found!");
        }

        private string GetShortAttributeWireConnected(string blockName)
        {
            if (blockName.Equals(Left.SourceBlockName) || blockName.Equals(Left.DestinationBlockName))
                return Left.AttributeWire;
            if (blockName.Equals(Down.SourceBlockName) || blockName.Equals(Down.DestinationBlockName))
                return Down.AttributeWire;
            if (blockName.Equals(Right.SourceBlockName) || blockName.Equals(Right.DestinationBlockName))
                return Right.AttributeWire;
            if (blockName.Equals(Up.SourceBlockName) || blockName.Equals(Up.DestinationBlockName))
                return Up.AttributeWire;

            throw new Exception("GetAttributeMultiwireConnected. Block name not found!");
        }

        

        private Point3d GetWirePoint()
        {
            throw new NotImplementedException();
        }

        private Point3d GetMultiWirePoint(ObjectId LinkSimbolId)
        {
            using var tr = _db.TransactionManager.StartTransaction();
            var blockRef = (BlockReference)tr.GetObject(LinkSimbolId, OpenMode.ForRead);
            var attributes = blockRef.AttributeCollection;
            foreach (ObjectId attributeId in attributes)
            {
                AttributeReference attRef = (AttributeReference)tr.GetObject(attributeId, OpenMode.ForWrite);
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
                if(polyline.EndPoint.IsEqualTo(PointConnectedToMultiWire))
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
            return new List<FakeAttribute>
            {
                new FakeAttribute{Tag = "X8_TINY_DOT_DONT_REMOVE", Layer = "MISC"},
                new FakeAttribute{Tag = "X2_TINY_DOT_DONT_REMOVE", Layer = "MISC"},
                new FakeAttribute{Tag = "X2TERM02", Layer = "MISC"},
                new FakeAttribute{Tag = "X2TERM01", Layer = "MISC"},
                new FakeAttribute{Tag = "SIGCODE", Value = SigCode, Layer = "MISC"},
                new FakeAttribute{Tag = "XREF", Value = "", Layer = "XREF"},
                new FakeAttribute{Tag = "DESC1", Value = ShortDescription, Layer = "DESC"},
                new FakeAttribute{Tag = "WIRENO", Value = "", Layer = "WIREREF"}
            };
        }

        private string GetSourceSymbolBlockName()
        {
            return _direction switch
            {
                Direction.Left => Left.SourceBlockName,
                Direction.Below => Down.SourceBlockName,
                Direction.Right => Right.SourceBlockName,
                Direction.Above => Up.SourceBlockName,
                _ => throw new Exception("Unknown direction!"),
            };
        }

        private string GetDestinationSymbolBlockName()
        {
            return _direction switch
            {
                Direction.Left => Left.DestinationBlockName,
                Direction.Below => Down.DestinationBlockName,
                Direction.Right => Right.DestinationBlockName,
                Direction.Above => Up.DestinationBlockName,
                _ => throw new Exception("Unknown direction!"),
            };
        }

        private bool TryGetExistingLinkSymbol(Entity wire, out ObjectId output)
        {
            var strings = new List<string>()
            {
                Left.SourceBlockName,
                Left.DestinationBlockName,
                Right.SourceBlockName,
                Right.DestinationBlockName,
                Down.SourceBlockName,
                Down.DestinationBlockName,
                Up.SourceBlockName,
                Up.DestinationBlockName
            };
            var blockRefIds = GetObjectsUtils.GetBlockIdsByNames(_db, strings);
            using var tr = _db.TransactionManager.StartTransaction();
            
            foreach (var blkRefId in blockRefIds) 
            {

                var attributesCollection = ((BlockReference)(blkRefId.GetObject(OpenMode.ForRead, false))).AttributeCollection;
                
                foreach(ObjectId attrId in attributesCollection)
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
                using var acadTr = _db.TransactionManager.TopTransaction;
                var result = AttributeHelper.SetBlockFakeAttributes(acadTr, existingLinkSymbolId, attributes);
                Debug.WriteLine("Ids = " + existingLinkSymbolId.ToString());
            }
            else
            {
                var linkSymbolId = BlockUtils.InsertBlockFormFile(db, symbolBlockName, PointConnectedToMultiWire, attributes, Layers.Symbols);
                var originSymbolPoint = ((BlockReference)linkSymbolId.GetObject(OpenMode.ForRead)).Position;
                var shiftPoint = GetMultiWirePoint(linkSymbolId);
                Debug.WriteLine("Original X: " + originSymbolPoint.X + " -> " + shiftPoint.X);
                Debug.WriteLine("Original Y: " + originSymbolPoint.Y + " -> " + shiftPoint.Y);
                Debug.WriteLine("Original Z: " + originSymbolPoint.Z + " -> " + shiftPoint.Z);
                var moveVec = new Vector3d(originSymbolPoint.X - shiftPoint.X, originSymbolPoint.Y - shiftPoint.Y, originSymbolPoint.Z - shiftPoint.Z);
                BlockUtils.MoveBlockReference(_db, linkSymbolId, moveVec);
            }

            //LinkerHelper.EraseBlockIfExist(db, PointConnectedToMultiWire);
            
        }

        

        public string GetWireDescription(Point3d point3D)
        {
            using var btr = (BlockTableRecord)SymbolUtilityServices.GetBlockModelSpaceId(_db).GetObject(OpenMode.ForRead);

            var rxClass = RXClass.GetClass(typeof(BlockReference));
            foreach (ObjectId id in btr)
            {
                if (id.ObjectClass.IsDerivedFrom(rxClass))
                {
                    var blkref = (BlockReference)id.GetObject(OpenMode.ForRead);
                    var attributes = blkref.AttributeCollection;
                    foreach (ObjectId attId in attributes)
                    {
                        var attref = (AttributeReference)attId.GetObject(OpenMode.ForRead);

                        if (!attref.Position.Equals(point3D))
                            continue;

                        if (IsOneOfTerminalLinks(attref.Tag))
                        {
                            var terminalNumberString = attref.Tag.Substring(6); // Exclude X?TERM from X?TERM01
                            var terminalDescriptionTag = "";
                            // If it is terminal, we must read attribute DESC1 for net name
                            if (blkref.Name.StartsWith("VT0002_") || blkref.Name.StartsWith("HT0002_"))
                                terminalDescriptionTag = "DESC1";
                            else
                                terminalDescriptionTag = "TERM" + terminalNumberString;

                            foreach (ObjectId objectId in attributes)
                            {
                                AttributeReference terminalAttr = (AttributeReference)objectId.GetObject(OpenMode.ForRead);
                                if (terminalAttr.Tag.Equals(terminalDescriptionTag))
                                {
                                    ComponentId = blkref.ObjectId;
                                    return terminalAttr.TextString;
                                }
                            }
                        }
                    }
                }
            }
            throw new Exception("Terminal attribute on this place wasn't found");
        }

        public Entity WireEntity { get; set; }

        public Point3d PointConnectedToMultiWire { get; set; }

        public HalfWire(Entity wireEntity)
        {
            if (wireEntity == null)
                return;

            WireEntity = wireEntity;

            _db = Application.DocumentManager.MdiActiveDocument.Database;
            Tolerance.Global = new Tolerance(1e-8, 1e-1);

            var wireEntities = GeometryFunc.GetAllConjugatedCurves(_db, (Curve)wireEntity, Layers.Wires);

            SetWireMultiwireCross((Curve)WireEntity);

            foreach (var entity in wireEntities.Cast<Curve>())
            {
                SetWireComponentCross(entity);
            }

            var zeroPoint = GetZeroPoint(WireEntity);
                       
            _direction = GeometryFunc.GetDirection(zeroPoint, GetEndPoint(WireEntity));
        }

        private bool SetWireComponentCross(Entity wireEntity)
        {
            var blockIds = GetObjectsUtils.GetObjects<BlockReference>(_db, Layers.Symbols).ToList();

            if (blockIds == null || blockIds.Count() == 0)
                return false;

            var components = AttributeHelper.GetObjectsStartWith(_db, "TERM");
            
            foreach(var component in components)
            {
                
                if (TryGetPointWhereWireConnectedToComponent(component, wireEntity))
                {
                    Debug.WriteLine("Point: X = " + PointConnectedToComponent.X + "; Y = " + PointConnectedToComponent.Y + "    " + Description);
                    break;
                }    
            }

            return false;
        }

        private bool TryGetPointWhereWireConnectedToComponent(BlockReference component, Entity wireEntity)
        {
            var curve = (Curve)wireEntity;
            var startPoint = curve.StartPoint;
            var endPoint = curve.EndPoint;


            var blkref = (BlockReference)component;
            var attributes = blkref.AttributeCollection;
            foreach (ObjectId attrId in attributes)
            {
                var attref = (AttributeReference)attrId.GetObject(OpenMode.ForRead, false);
                if (!attref.Position.Equals(startPoint) && !attref.Position.Equals(endPoint))
                    continue;

                PointConnectedToComponent = startPoint;
                    
                if (attref.Position.Equals(startPoint))
                {
                    PointConnectedToComponent = endPoint;
                }
                    
                GetDescription(blkref, attributes, attref);
                    
                return true;
            }
            
            
            return false;
        }

        // Get terminal description in point where a wire connected to the component
        private void GetDescription(BlockReference blkref, AttributeCollection attributes, AttributeReference attref)
        {
            if (IsOneOfTerminalLinks(attref.Tag))
            {
                var terminalNumberString = attref.Tag.Substring(6); // Exclude X?TERM from X?TERM01
                var terminalDescriptionTag = "";
                // If it is terminal, we must read attribute DESC1 for net name
                if (blkref.Name.StartsWith("VT0002_") || blkref.Name.StartsWith("HT0002_"))
                    terminalDescriptionTag = "DESC1";
                else
                    terminalDescriptionTag = "TERM" + terminalNumberString;

                foreach (ObjectId objectId in attributes)
                {
                    AttributeReference terminalAttr = (AttributeReference)objectId.GetObject(OpenMode.ForRead);
                    if (terminalAttr.Tag.Equals(terminalDescriptionTag))
                    {
                        ComponentId = blkref.ObjectId;
                        Description = terminalAttr.TextString;
                    }
                }
            }
        }

        public void SetWireMultiwireCross(Curve wireEntity)
        {
            var multiWires = new List<Curve>();
            multiWires.AddRange(GetObjectsUtils.GetObjects<Polyline>(_db, Layers.MultiWires));

            multiWires.AddRange(GetObjectsUtils.GetObjects<Line>(_db, Layers.MultiWires));

            if (multiWires == null || multiWires.Count() == 0)
            {
                return;
            }

            foreach(var multiWire in multiWires)
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

        // Source/Destination Wire Signal Symbol
        public Entity LinkSymbol { get; set; }
        public Point3d PointConnectedToComponent { get; private set; }

        public ObjectId ComponentId { get; private set; }

        public string ShortDescription { get; set; }

        public void Clean()
        {
            LinkSymbol.Erase();
        }
    }
}
