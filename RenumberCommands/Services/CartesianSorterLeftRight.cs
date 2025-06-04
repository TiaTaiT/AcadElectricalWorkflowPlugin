using System.Collections.Generic;
using System.Linq;
using RenumberCommands.Interfaces;

namespace RenumberCommands.Services
{
    public class CartesianSorterLeftRight : IBlockSorter
    {
        public IEnumerable<BlockReference> Sort(IEnumerable<BlockReference> blocks)
        {
            // Assumes Position is the insertion point
            return blocks.OrderBy(br => br.Position.X)
                         .ThenByDescending(br => br.Position.Y);
        }
    }
}
