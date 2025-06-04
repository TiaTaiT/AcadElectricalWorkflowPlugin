using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutocadCommands.Helpers;

using CommonHelpers;

namespace RenumberCommands.Services
{
    public class BlocksTagsRenumberer : CommandPrototype
    {
        private const string _fixedSymbol = "F";
        private const string _tagNamePreffix = "TAG";
        public BlocksTagsRenumberer(Document doc) : base(doc)
        {
        }

        public override bool Init()
        {
            return true;
        }

        public override void Run()
        {
            using var acLckDoc = _doc.LockDocument();
            

                var blkRefsWithNoFixedTag = 
                    AttributeHelper.GetObjectsWithAttributeAndLayer(_db, _tr, _tagNamePreffix, Layers.Tags);
                // blkRefsWithNoFixedTag contains terminals, because of they contain "TAGSTRIP" attribute
                // we need to remove terminals
                var blkRefsWithTagsOnly = getBlocksWithNoFixedTag(blkRefsWithNoFixedTag);

                BlocksRenumbering(blkRefsWithTagsOnly);


                foreach (var blkRef in blkRefsWithTagsOnly)
                {
                    var attrCol = blkRef.AttributeCollection;
                    var tagValue = AttributeHelper.GetAttributeValueStartWith(attrCol, "TAG1");
                    
                    Debug.WriteLine($"Found {tagValue}");
                }
            
        }

        private IEnumerable<BlockReference> getBlocksWithNoFixedTag(IEnumerable<BlockReference> blockRefs)
        {
            foreach (var blkRef in blockRefs)
            {
                if (blkRef.IsErased) continue;

                if (blkRef.AttributeCollection.Count == 0) continue;

                var attributes = blkRef.AttributeCollection;

                foreach (ObjectId attrId in attributes)
                {
                    var attrRef = (AttributeReference)attrId.GetObject(OpenMode.ForRead, false);
                    if (attrRef == null) continue;
                    if (attrRef.Tag.Length <= _tagNamePreffix.Length) continue;
                    if (!attrRef.Tag.EndsWith(_fixedSymbol) && Char.IsDigit(attrRef.Tag[3]))
                    {
                        yield return blkRef;
                        break;
                    }
                }
            }
        }

        public static void BlocksRenumbering(IEnumerable<BlockReference> blocks)
        {
            var sorter = new CartesianSorterUpDown();
            var parser = new SuffixTagParser();
            var service = new BlockRenumberingService(sorter, parser);
            service.RenumberBlocks(blocks);
        }
    }
}
