using System.Collections.Generic;
using Teigha.DatabaseServices;

namespace RenumberCommands.Interfaces
{
    public interface IBlockSorter
    {
        IEnumerable<BlockReference> Sort(IEnumerable<BlockReference> blocks);
    }
}
