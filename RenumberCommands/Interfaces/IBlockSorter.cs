using System.Collections.Generic;

namespace RenumberCommands.Interfaces
{
    public interface IBlockSorter
    {
        IEnumerable<BlockReference> Sort(IEnumerable<BlockReference> blocks);
    }
}
