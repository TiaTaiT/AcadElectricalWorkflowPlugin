using AutocadCommands.Models;
using LinkCommands.Models;
using System.Collections.Generic;
using System.Linq;

namespace LinkCommands.Services
{
    public class HalfWirePairSeeker
    {
        private IEnumerable<ElectricalComponent> _components;
        private NamesConverter _namesConverter;
        private List<HalfWire> _sources;
        private List<HalfWire> _destinations;

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="components">Existed electrical components</param>
        public HalfWirePairSeeker(IEnumerable<ElectricalComponent> components)
        {
            _components = components;
            _namesConverter = new NamesConverter();
        }

        public IEnumerable<(HalfWire, HalfWire)> GetPairs(
               List<HalfWire> sources, List<HalfWire> destinations) 
        {
            _sources = sources;
            _destinations = destinations;
            
            var pairs = new List<(HalfWire, HalfWire)>();

            for(var i = 0; i < _sources.Count(); i++)
            {
                for(var j = 0; j < _destinations.Count(); j++)
                {
                    var source = _sources[i];
                    var destination = _destinations[j];
                    if (TryCheckLinkAllow(source, destination, out string description))
                    { 
                        source.ShortDescription = description;
                        destination.ShortDescription = description;
                        pairs.Add( (source, destination) );
                        _sources.RemoveAt(i);
                        _destinations.RemoveAt(j);
                        i = -1;
                        
                        break;
                    }
                }
            }
            return pairs;
        }

        private bool TryCheckLinkAllow(HalfWire source, HalfWire destination, out string description)
        {
            description = string.Empty;
            var sComponnet = GetAttachedComponent(source);
            var dComponnet = GetAttachedComponent(destination);
            if (sComponnet == null || dComponnet == null)
            {
                return false;
            }

            IEnumerable<Wire> sourceTiedWires = GetTiedWires(sComponnet, source.Terminal);
            IEnumerable<Wire> destTiedWires = GetTiedWires(dComponnet, destination.Terminal);

            if(!sourceTiedWires.Any() && !destTiedWires.Any()) 
                return false;

            string sourceDescription;
            string destinationDescription;
            if (!sourceTiedWires.Any() && destTiedWires.Any())
            {
                sourceDescription = source.Terminal.Value;
                destinationDescription = GetDescritpion(destTiedWires);

            }
            else if(!destTiedWires.Any() && sourceTiedWires.Any())
            {
                destinationDescription = destination.Terminal.Value;
                sourceDescription = GetDescritpion(sourceTiedWires);
            }
            else 
            {
                sourceDescription = GetDescritpion(sourceTiedWires);
                destinationDescription = GetDescritpion(destTiedWires);
            }

            var validator = new ElectricalValidation(new DesignationParser(), _namesConverter)
            {
                ValidationParameterIsTerminal = sComponnet.IsTerminal || dComponnet.IsTerminal,
            };
            
            if (validator.IsConnectionValid(sourceDescription, destinationDescription))
            {
                description = validator.ShortName;
                return true;
            }
            return false;
        }

        //Need to loop for finding description
        private static string GetDescritpion(IEnumerable<Wire> tiedWires)
        {
            foreach(var wire in tiedWires)
            {
                if (string.IsNullOrEmpty(wire.Description))
                {
                    continue;
                }
                return wire.Description;
            }
            return tiedWires.FirstOrDefault().Terminals.FirstOrDefault().Value;
        }

        private IEnumerable<Wire> GetTiedWires(ElectricalComponent sComponnet, ComponentTerminal terminal)
        {
            var tiedWires = new List<Wire>();
            foreach (var tiedTerminal in sComponnet.GetAllTiedTerminals(terminal.Value))
            {
                tiedWires.AddRange( tiedTerminal.ConnectedWires );
            }
            return tiedWires;
        }

        private ElectricalComponent GetAttachedComponent(HalfWire halfWire)
        {
            foreach (var component in _components)
            {
                foreach(var terminal in component.Terminals)
                {
                    if (halfWire.Terminal == terminal)
                        return component;
                }
            }
            return null;
        }
    }
}
