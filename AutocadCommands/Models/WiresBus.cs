using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutocadCommands.Models
{
    public class WiresBus
    {
        public IEnumerable<FullWire> FullWires { get; set; }
        public IEnumerable<Polyline> WireBus { get; set; }
    }
}
