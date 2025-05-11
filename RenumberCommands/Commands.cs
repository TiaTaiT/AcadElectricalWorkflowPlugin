using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RenumberCommands.Services;

using Bricscad.ApplicationServices;
using Teigha.Runtime;
using Teigha.DatabaseServices;

namespace RenumberCommands
{
    public class Commands
    {
        // Advanced blocks increment
        [CommandMethod("TAGSRENUMBER")]
        public void TagsRenumbering()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                var blockRefs = new List<BlockReference>();
                foreach (ObjectId entId in ms)
                {
                    if (!(tr.GetObject(entId, OpenMode.ForRead) is BlockReference br))
                        continue;

                    // 1) must be on layer "SYMS"
                    if (!string.Equals(br.Layer, "SYMS", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // collect all attributes of this block
                    var atts = br.AttributeCollection
                                .Cast<ObjectId>()
                                .Select(id => tr.GetObject(id, OpenMode.ForRead) as AttributeReference)
                                .ToList();

                    // 2) must have a TAG1 attribute on layer "TAGS"
                    bool hasTag1OnTagsLayer = atts.Any(a =>
                        string.Equals(a.Tag, "TAG1", StringComparison.OrdinalIgnoreCase)
                     && string.Equals(a.Layer, "TAGS", StringComparison.OrdinalIgnoreCase));

                    if (!hasTag1OnTagsLayer)
                        continue;

                    // 3) must NOT have ANY TAGSTRIP attribute
                    bool hasTagStrip = atts.Any(a =>
                        string.Equals(a.Tag, "TAGSTRIP", StringComparison.OrdinalIgnoreCase));

                    if (hasTagStrip)
                        continue;

                    blockRefs.Add(br);
                }

                if (blockRefs.Count == 0)
                {
                    ed.WriteMessage("\nNo eligible blocks found (SYMS layer, TAG1 on TAGS, excluding TAGSTRIP).");
                }
                else
                {
                    // call your existing renumbering routine
                    BlocksTagsRenumberer.BlocksRenumbering(blockRefs);

                    tr.Commit();
                    ed.WriteMessage($"\nRenumbered {blockRefs.Count} blocks on SYMS layer.");
                }
            }

        }
    }
}
