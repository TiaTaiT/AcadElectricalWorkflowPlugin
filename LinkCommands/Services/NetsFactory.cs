using AutocadCommands.Helpers;
using AutocadCommands.Models;
using AutocadCommands.Services;
using CommonHelpers;
using LinkCommands.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Teigha.DatabaseServices;
using Teigha.Geometry;

namespace LinkCommands.Services
{
    public class NetsFactory
    {
        private readonly Database _db;
        private readonly IEnumerable<ElectricalComponent> _components;
        private const string _linkSign = "SIGCODE";
        private const string _descriptionSign = "DESC1";
        private IEnumerable<Point3d> _terminators;

        private IEnumerable<HalfWire> HalfWires { get; set; }
        public IEnumerable<Wire> Wires { get; private set; }


        public NetsFactory(Database db, IEnumerable<Point3d> terminators)
        {
            _db = db;
            _terminators = terminators;

            FindElectricalNets();
        }

        private void FindElectricalNets()
        {
            var curves = LinkerHelper.GetAllWiresFromDb(_db);
            var curveGroups = CreateConjugatedCurves(curves);
            var linkReferences = AttributeHelper.GetObjectsStartWith(_db, _linkSign);
            var links = GetLinkConnectionPointPairs(linkReferences);
            HalfWires = CreateHalfWires(curveGroups, links, out var restGroups);
            DebugHalfWires();
            Debug.WriteLine("Rest wires groups count = " + restGroups.Count());
            var wires = CreateWires(HalfWires).ToList();
            Debug.WriteLine("Number of wires: " + wires.Count());
            Debug.WriteLine("");
            var validWiresGroups = GetValidWiresGroups(restGroups);
            Debug.WriteLine("Rest valid wires group = " + validWiresGroups.Count());
            wires.AddRange(GeWires(validWiresGroups));
            Wires = wires;
            Debug.WriteLine("Total wires = " + Wires.Count());
        }

        private IEnumerable<Wire> GeWires(IEnumerable<IEnumerable<Curve>> validWiresGroups)
        {
            var wires = new List<Wire>();
            foreach (var wiresGroup in validWiresGroups)
            {
                wires.Add(new Wire(wiresGroup));
            }
            return wires;
        }

        /// <summary>
        /// Method return groups contain valid wires (The wire connecting at least two terminals) 
        /// </summary>
        /// <param name="curveGroups">Rest part of curve groups (after halfWiring)</param>
        /// <returns>Valid groups of curves for wires creating</returns>
        private IEnumerable<IEnumerable<Curve>> GetValidWiresGroups(IEnumerable<IEnumerable<Curve>> curveGroups)
        {
            var wires = new List<IEnumerable<Curve>>();
            foreach (var curveGroup in curveGroups)
            {
                var terminatorsCounter = 0;
                foreach (var curve in curveGroup)
                {
                    terminatorsCounter += GetNumbTerminatedEnds(curve);
                    if (terminatorsCounter == 0)
                        continue;

                    if (terminatorsCounter >= 2)
                    {
                        wires.Add(curveGroup);
                        break;
                    }
                }
            }
            return wires;
        }

        private void DebugHalfWires()
        {
            Debug.WriteLine("HallfWires.Count = " + HalfWires.Count());
            foreach (HalfWire halfWire in HalfWires)
            {
                Debug.WriteLine("DESC1 = " + halfWire.Description + "  Point: " + halfWire.LinkSymbol.Position.X.ToString() + ";" + halfWire.LinkSymbol.Position.Y.ToString() + "  Count = " + halfWire.Curves.Count());
            }
        }

        private IEnumerable<Wire> CreateWires(IEnumerable<HalfWire> halfWires)
        {
            var wires = new List<Wire>();

            var groups = halfWires.GroupBy(h => h.SigCode).ToList();

            foreach (var group in groups)
            {
                var halfs = group.ToList();
                if (halfs.Count == 2)
                {
                    wires.Add(new Wire(halfs[0], halfs[1]));
                }
            }

            return wires;
        }

        private IEnumerable<Link> GetLinkConnectionPointPairs(IEnumerable<BlockReference> linkReferences)
        {
            var result = new List<Link>();
            var possibleWireAttachedNames = LinkStruct.GetPossibleWireAttachedNames();
            var possibleMultiwireAttachedNames = LinkStruct.GetPossibleMultiwireAttachedNames();
            foreach (var linkRef in linkReferences)
            {
                var attCol = linkRef.AttributeCollection;

                var link = new Link();
                foreach (var attributeName in possibleWireAttachedNames)
                {
                    if (!AttributeHelper.TryGetAttributePosition(attributeName, attCol, out var wirePoint))
                        continue;

                    link.WireConnectionPoint = wirePoint;
                    link.Description = AttributeHelper.GetAttributeValueStartWith(attCol, _descriptionSign);
                    link.SigCode = AttributeHelper.GetAttributeValueStartWith(attCol, _linkSign);
                    link.Reference = linkRef;

                    break;
                }
                foreach (var attributeName in possibleMultiwireAttachedNames)
                {
                    if (!AttributeHelper.TryGetAttributePosition(attributeName, attCol, out var multiwirePoint))
                        continue;
                    link.MultiwireConnectionPoint = multiwirePoint;
                    break;
                }

                if (!string.IsNullOrEmpty(link.SigCode))
                    result.Add(link);
            }
            return result;
        }

        /// <summary>
        /// Method creates "half wires" from wires groups
        /// </summary>
        /// <param name="curveGroups">All groups of conjugated wires were founded in the database</param>
        /// <param name="links">All link-symbols from current databse</param>
        /// <param name="restGroups">Rest part of grous left after all half wires was created (this groups isn't parts of the half wires)</param>
        /// <returns>All valid "HalfWires"</returns>
        private IEnumerable<HalfWire> CreateHalfWires(IEnumerable<IEnumerable<Curve>> curveGroups,
                                                      IEnumerable<Link> links,
                                                      out IEnumerable<IEnumerable<Curve>> restGroups)
        {
            var halfWires = new List<HalfWire>();
            var restWiresGroups = new LinkedList<IEnumerable<Curve>>(curveGroups);

            foreach (var curveGroup in curveGroups)
            {
                foreach (var curve in curveGroup)
                {
                    foreach (var link in links)
                    {
                        if (!GeometryFunc.IsPointOnLine(curve, link.WireConnectionPoint))
                            continue;
                        halfWires.Add(new HalfWire(curveGroup, link));
                        restWiresGroups.Remove(curveGroup);
                        break;
                    }
                }
            }
            restGroups = restWiresGroups;
            return halfWires;
        }

        private IEnumerable<IEnumerable<Curve>> CreateConjugatedCurves(IEnumerable<Curve> allCurves)
        {
            var curves = allCurves.ToList();
            var CurveGroups = new List<List<Curve>>();
            while (curves.Any())
            {
                var curve = curves.First();
                CurveGroups.Add(GeometryFunc.MoveConjugatedCurves(curve, curves, _terminators).ToList());
                //Debug.WriteLine("Remained in the collection: " + curves.Count);
            }
            return CurveGroups;
        }

        private int GetNumbTerminatedEnds(Curve curve)
        {
            var terminatedWireEnds = 0;
            foreach (var terminator in _terminators)
            {
                if (curve.StartPoint.Equals(terminator) || curve.EndPoint.Equals(terminator))
                {
                    terminatedWireEnds++;
                }
            }
            return terminatedWireEnds;
        }
    }
}
