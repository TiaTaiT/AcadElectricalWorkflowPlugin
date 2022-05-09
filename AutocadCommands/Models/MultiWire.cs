using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutocadCommands.Models
{
    internal class MultiWire
    {
        public Entity MultiWireEntity { get; set; }

        public List<Wire> ConnectedWires { get; set; }
    }
}
