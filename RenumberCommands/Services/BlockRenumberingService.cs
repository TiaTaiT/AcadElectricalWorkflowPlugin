using System;
using System.Collections.Generic;
using System.Linq;
using RenumberCommands.Interfaces;
using Teigha.DatabaseServices;

namespace RenumberCommands.Services
{
    public class BlockRenumberingService
    {
        private readonly IBlockSorter _sorter;
        private readonly ITagParser _parser;

        public BlockRenumberingService(IBlockSorter sorter, ITagParser parser)
        {
            _sorter = sorter ?? throw new ArgumentNullException(nameof(sorter));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public void RenumberBlocks(IEnumerable<BlockReference> blockRefs)
        {
            // Group by parsed prefix
            var items = blockRefs.Select(br =>
            {
                var att = br.AttributeCollection
                            .Cast<ObjectId>()
                            .Select(id => id.GetObject(OpenMode.ForRead) as AttributeReference)
                            .First(a => a.Tag.StartsWith("TAG", StringComparison.OrdinalIgnoreCase));
                var (prefix, _) = _parser.Parse(att.TextString);
                return new { Block = br, Attribute = att, Prefix = prefix };
            });

            var groups = items.GroupBy(x => x.Prefix, StringComparer.OrdinalIgnoreCase);

            foreach (var group in groups)
            {
                // Sort actual BlockReference objects
                var sortedBlocks = _sorter.Sort(group.Select(x => x.Block)).ToList();

                for (int i = 0; i < sortedBlocks.Count; i++)
                {
                    var br = sortedBlocks[i];
                    // Find the attribute for this block and prefix
                    var attRef = group
                        .First(x => x.Block == br)
                        .Attribute;
                    attRef.UpgradeOpen();          // for write
                    attRef.TextString = $"{group.Key}{i + 1}";
                    attRef.DowngradeOpen();
                }
            }
        }
    }
}
