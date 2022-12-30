using AutocadCommands.Services;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using CommonHelpers;
using LinkCommands.Models;
using LinkCommands.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private IEnumerable<ElectricalComponent> _components;

        private void CreateMultiwire()
        {
            var multiwires = new List<Curve>() { Multiwire };
            ConnectedWires = GetConnectedWires(multiwires, _allWires).ToList();

            if (ConnectedWires == null || ConnectedWires.Count() == 0)
            {
                //Debug.WriteLine("Wires count = " + ConnectedWires.Count() + ";   Operation halt!");
                return;
            }
            //Debug.WriteLine("Wires count = " + ConnectedWires.Count());

            var sortedHalfWires = GetSortHalfWire();
            var separator = new Separator();

            separator.Separate(sortedHalfWires);

            _sourceHalfWires = separator.Sources;
            _destinationHalfWires = separator.Destinations;
        }

        private bool CreateWires()
        {
            var result = false;
            if (_sourceHalfWires == null || _destinationHalfWires == null)
                return result;

            var seeker = new HalfWirePairSeeker(_components);

            var foundSources = new List<HalfWire>();
            var foundDestinations = new List<HalfWire>();
            foreach (var pair in seeker.GetPairs(_sourceHalfWires, _destinationHalfWires))
            {
                foundSources.Add(pair.Item1);
                foundDestinations.Add(pair.Item2);
            }
            DebugPairs(foundSources, foundDestinations);

            if (CreateWires(foundSources, foundDestinations))
            {
                // We need delete created halfWires from common collection (_sourceHalfWires and _destinationHalfWires)
                // To avoid double create of wires 
                DeleteFoundWires(foundSources, foundDestinations);
            }

            var max = _sourceHalfWires.Count();

            if (_sourceHalfWires.Count() > _destinationHalfWires.Count())
                max = _destinationHalfWires.Count();

            for (var i = 0; i < max; i++)
            {
                var wire = new Wire(_sourceHalfWires[i], _destinationHalfWires[i], _components);
                wire.Create();
                result = true;
            }

            return result;
        }

        private void DeleteFoundWires(IEnumerable<HalfWire> foundSources, IEnumerable<HalfWire> foundDestinations)
        {
            foreach (var foundSource in foundSources)
            {
                _sourceHalfWires.Remove(foundSource);
            }
            foreach (var foundDestination in foundDestinations)
            {
                _sourceHalfWires.Remove(foundDestination);
            }
        }

        private bool CreateWires(IEnumerable<HalfWire> sources, IEnumerable<HalfWire> destination)
        {
            if (sources.Count() != destination.Count())
                return false;

            for (var i = 0; i < sources.Count(); i++)
            {
                var wire = new Wire(sources.ElementAt(i), destination.ElementAt(i), _components);
                wire.Create();
            }

            return true;
        }

        private static void DebugPairs(IEnumerable<HalfWire> sources, IEnumerable<HalfWire> destinations)
        {
            Debug.WriteLine("sources found = " + sources.Count() + "; destinations found = " + destinations.Count());

            for (var i = 0; i < sources.Count(); i++)
            {
                Debug.WriteLine(sources.ElementAt(i).ShortDescription + " <=> " + destinations.ElementAt(i).ShortDescription);
            }
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
                connectedToMultiwireWires.Add(new HalfWire(wire, _components));
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
        public MultiWire(Polyline polyLine, IEnumerable<ElectricalComponent> components)
        {
            _components = components;
            Tolerance.Global = new Tolerance(1e-8, 1e-1);
            Multiwire = polyLine;
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            _allWires = LinkerHelper.GetAllWiresFromDb(db);
            CreateMultiwire();
        }

        public MultiWire(IEnumerable<Entity> sourceEntities,
                         Entity sourceLinkSymbol,
                         IEnumerable<Entity> destinationEntities,
                         Entity destinationLinkSymbol,
                         IEnumerable<ElectricalComponent> components)
        {
            _components = components;
            Tolerance.Global = new Tolerance(1e-8, 1e-1);
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
            Source?.Clean();
            Destination?.Clean();
        }
    }
}
