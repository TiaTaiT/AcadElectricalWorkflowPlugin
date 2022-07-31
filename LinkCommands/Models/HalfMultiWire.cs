using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkCommands.Models
{
    public class HalfMultiWire
    {
        public IEnumerable<Entity> WireSegments { get; set; }

        public Entity LinkSymbol { get; set; }

        public void Clean()
        {
            LinkSymbol.Erase();
        }
    }
}
