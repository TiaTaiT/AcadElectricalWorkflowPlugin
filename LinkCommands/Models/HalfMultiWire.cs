using System.Collections.Generic;

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
