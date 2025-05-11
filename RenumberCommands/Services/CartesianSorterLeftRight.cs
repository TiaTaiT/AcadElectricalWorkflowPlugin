using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RenumberCommands.Interfaces;
using Teigha.DatabaseServices;

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
