using AutocadCommands.Models;
using System.Collections.Generic;

namespace LinkCommands.Models
{
    public static class ComponentsWiresTier
    {
        /// <summary>
        /// The method connects the terminals of the devices with the wires. Method modifies the input collections.
        /// </summary>
        /// <param name="terminals"></param>
        /// <param name="wires"></param>
        public static void CreateElectricalNet(IEnumerable<ComponentTerminal> terminals, IEnumerable<Wire> wires)
        {

            foreach (var terminal in terminals)
            {
                foreach (var point in terminal.Points)
                {
                    foreach (var wire in wires)
                    {
                        if (wire.IsPointOnEnd(point))
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
