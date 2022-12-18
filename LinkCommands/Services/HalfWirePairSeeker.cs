using AutocadCommands.Models;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.MacroRecorder;
using LinkCommands.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Autodesk.AutoCAD.DatabaseServices.RenderGlobal;

namespace LinkCommands.Services
{
    public class HalfWirePairSeeker
    {
        private IEnumerable<ElectricalComponent> _components;
        private Wire _wires;
        private List<HalfWire> _sources;
        private List<HalfWire> _destinations;

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="components">Existed electrical components</param>
        public HalfWirePairSeeker(IEnumerable<ElectricalComponent> components)
        {
            _components = components;
            
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
                destinationDescription = destTiedWires.FirstOrDefault().Source.Description;

            }
            else if(!destTiedWires.Any() && sourceTiedWires.Any())
            {
                destinationDescription = destination.Terminal.Value;
                sourceDescription = sourceTiedWires.FirstOrDefault().Source.Description;
            }
            else 
            {
                sourceDescription = sourceTiedWires.FirstOrDefault().Source.Description;
                destinationDescription = destTiedWires.FirstOrDefault().Source.Description;
            }
            /*
            if(sComponnet.IsTerminal || dComponnet.IsTerminal)
            {
                if(IsShortNamesAreEqual(sourceDescription, destinationDescription, out var shortTerminalName))
                {
                    description= shortTerminalName;
                    return true;
                }
                return false;
            }
            */
            if (IsDescriptionsCompatible(sourceDescription, destinationDescription, out var shortName))
            {
                description = shortName;
                return true;
            }
            return false;
        }

        private bool IsShortNamesAreEqual(string source, string destination, out string shortTerminalName)
        {
            var validator = new ElectricalValidation()
            {
                ValidationParameterIsTerminal = true
            };

            var validateResult = validator.ValidateWire(source, destination);
            shortTerminalName = validator.ShortName;

            return validateResult;
        }

        private bool IsDescriptionsCompatible(string sourceDescription, string destinationDescription, out string shortName)
        {
            shortName = string.Empty;
            var validator = new ElectricalValidation();

            if (validator.ValidateWire(sourceDescription, destinationDescription))
            {
                shortName = NamesConverter.GetShortAlias(sourceDescription, destinationDescription);
                return true;
            }
            return false;
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
