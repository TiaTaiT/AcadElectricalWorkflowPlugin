using AutocadCommands.Services;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using CommonHelpers;
using LinkCommands.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace AutocadCommands.Models
{
    public class MultiWire
    {
        private List<Curve> ConnectedWires;
        private List<HalfWire> _sourceHalfWires = new();
        private List<HalfWire> _destinationHalfWires = new();
        private List<Wire> _wires = new();
        private IEnumerable<Curve> _allWires;

        private void CreateMultiwire()
        {
            var multiwires = new List<Curve>() { Multiwire };
            ConnectedWires = GetConnectedWires(multiwires, _allWires).ToList();
            
            if (ConnectedWires == null || ConnectedWires.Count() == 0)
            {
                Debug.WriteLine("Wires count = " + ConnectedWires.Count() + ";   Operation halt!");
                return;
            }
            Debug.WriteLine("Wires count = " + ConnectedWires.Count());

            var sortedHalfWires = GetSortHalfWire();
            if (!SeparateSourceAndDestination(sortedHalfWires))
                return; 
        }

        private bool CreateWires()
        {
            var result = false;
            if (_sourceHalfWires == null || _destinationHalfWires == null)
                return result;

            var max = _sourceHalfWires.Count();

            if(_sourceHalfWires.Count() > _destinationHalfWires.Count())
                max = _destinationHalfWires.Count();

            for (var i = 0; i < max; i++)
            {
                var wire = new Wire(_sourceHalfWires[i], _destinationHalfWires[i]);
                wire.Create();
                result = true;
            }

            return result;
        }

        private bool SeparateSourceAndDestination(IEnumerable<HalfWire> sortedHalfWires)
        {
            if(sortedHalfWires.Count() < 2)
                return false;

            var sourceDestSwitch = false;
            var lastValue = sortedHalfWires.First().PointConnectedToMultiWire.Y;
            _sourceHalfWires.Add(sortedHalfWires.First());

            for(var i = 1; i < sortedHalfWires.Count(); i++)
            {
                var currentWireY = sortedHalfWires.ElementAt(i).PointConnectedToMultiWire.Y;
                
                if (currentWireY != lastValue)
                {
                    sourceDestSwitch = true;
                }
                if(!sourceDestSwitch)
                {
                    _sourceHalfWires.Add(sortedHalfWires.ElementAt(i));
                }
                else
                {
                    if(_sourceHalfWires.Count() > _destinationHalfWires.Count())
                    {
                        _destinationHalfWires.Add(sortedHalfWires.ElementAt(i));
                    }
                }
                lastValue = currentWireY;
            }

            // if multiwire is one line
            if(_sourceHalfWires.Count() > 0 && _destinationHalfWires.Count() == 0)
            {
                if(_sourceHalfWires.Count() % 2 != 0)
                    return false;

                // split source collection in half
                var halfNumb = _sourceHalfWires.Count() / 2 - 1;
                var max = _sourceHalfWires.Count() - 1;
                for (var j = max; j > halfNumb; j--)
                {
                    _destinationHalfWires.Add(_sourceHalfWires[j]);
                    _sourceHalfWires.RemoveAt(j);
                }
            }
            return true;
        }

        private void CreateLinkedMultiwire()
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            var allWireIds = LinkerHelper.GetAllWiresFromDb(db);

            ConnectedWires = new List<Curve>();
            ConnectedWires.AddRange(GetConnectedWires(Source.WireSegments.Cast<Line>(), allWireIds));
            _sourceHalfWires = GetSortHalfWire();

            ConnectedWires = new List<Curve>();
            ConnectedWires.AddRange(GetConnectedWires(Destination.WireSegments.Cast<Line>(), allWireIds));
            _destinationHalfWires = GetSortHalfWire();
        }

        private List<HalfWire> GetSortHalfWire()
        {
            var connectedToMultiwireWires = new List<HalfWire>();

            foreach (var wire in ConnectedWires)
            {
                ;
                connectedToMultiwireWires.Add(new HalfWire(wire));
            }
            connectedToMultiwireWires.Sort(new HalfWireComparer());
            return connectedToMultiwireWires;
        }

        
        private static IEnumerable<Curve> GetConnectedWires(IEnumerable<Curve> multiwireSegments, IEnumerable<Curve> allWires)
        {
            foreach (var multiwireSegment in multiwireSegments)
            {
                foreach (var wire in allWires)
                {
                    var points = LinkerHelper.GetStartEndPoints(wire);

                    if (GeometryFunc.IsPointOnLine(multiwireSegment, points.Item1) ||
                        GeometryFunc.IsPointOnLine(multiwireSegment, points.Item2))
                    {
                        yield return wire;
                    }
                }
            }
        }

        
        #region Constructors
        public MultiWire(Polyline polyLine)
        {
            Multiwire = polyLine;
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            _allWires = LinkerHelper.GetAllWiresFromDb(db);
            CreateMultiwire();
        }

        public MultiWire(IEnumerable<Entity> sourceEntities, 
                         Entity sourceLinkSymbol, 
                         IEnumerable<Entity> destinationEntities,
                         Entity destinationLinkSymbol)
        {
            Source = new HalfMultiWire
            {
                LinkSymbol = sourceLinkSymbol,
                WireSegments = sourceEntities,
            };

            Destination = new HalfMultiWire
            {
                LinkSymbol = destinationLinkSymbol,
                WireSegments = destinationEntities,
            };

            var db = Application.DocumentManager.MdiActiveDocument.Database;
            _allWires = LinkerHelper.GetAllWiresFromDb(db);
            CreateLinkedMultiwire();

            
        }
        #endregion Constructors

        /// <summary>
        /// All wires connected to multiwire
        /// </summary>
        public IEnumerable<Wire> Wires { get; set; }

        /// <summary>
        /// If multiwire consist from lines (not polylines!), this is the source part
        /// </summary>
        public HalfMultiWire Source { get; set; }

        /// <summary>
        /// If multiwire consist from lines (not polylines!), this is the destination part
        /// </summary>
        public HalfMultiWire Destination { get; set; }

        /// <summary>
        /// If multiwire consist from polylines (not lines!), this is the polyline
        /// </summary>
        public Polyline Multiwire { get; set; }

        /// <summary>
        /// Create new link source/destination symbols beside every wire and multiwire
        /// </summary>
        public bool Create()
        {
            return CreateWires();
        }

        /// <summary>
        /// Clean all symbol links and halfwire symbols
        /// </summary>
        public void Clean()
        {
            if(Source != null)
            {
                Source.Clean();
            }
            if (Destination != null)
            {
                Destination.Clean();
            }
        }
    }
}
