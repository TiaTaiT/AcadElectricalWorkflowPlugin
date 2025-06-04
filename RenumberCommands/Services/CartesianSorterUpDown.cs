using System.Collections.Generic;
using System.Linq;
using RenumberCommands.Interfaces;

namespace RenumberCommands.Services
{
    public class CartesianSorterUpDown : IBlockSorter
    {
        public IEnumerable<BlockReference> Sort(IEnumerable<BlockReference> blocks)
        {
            return blocks
                .OrderByDescending(br => br.Position.Y)  // up → down
                .ThenBy(br => br.Position.X);            // left → right
        }
    }
}
