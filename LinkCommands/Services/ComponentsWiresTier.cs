using AutocadCommands.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkCommands.Models
{
    public static class ComponentsWiresTier
    {
        public static void CreateElectricalNet(IEnumerable<ComponentTerminal> terminals, IEnumerable<Wire> wires) 
        {

            foreach(var terminal in terminals)
            {
                foreach(var point in terminal.Points)
                {
                    foreach(var wire in wires)
                    {
                        if(wire.IsPointOnEnd(point))
                        {
                            terminal.ConnectedWires.Add(wire);
                            wire.Terminals.Add(terminal);
                        }
                    }
                }
            }
        }
    }
}
