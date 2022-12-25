using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace AutocadTerminalsManager.Helpers
{
    public static class DrawOrderHelper
    {
        public static void WipeoutsToBottom()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            try
            {
                // Ask the user to select a block or None for "all"

                var peo = new PromptEntityOptions("\nSelect block to fix <all>");
                peo.SetRejectMessage("Must be a block.");
                peo.AddAllowedClass(typeof(BlockReference), false);
                peo.AllowNone = true;

                var per = ed.GetEntity(peo);

                if (per.Status != PromptStatus.OK && per.Status != PromptStatus.None)
                    return;

                // If the user hit enter, run on all blocks in the drawing

                bool allBlocks = per.Status == PromptStatus.None;

                doc.LockDocument();
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var toProcess = new ObjectIdCollection();
                    if (allBlocks)
                    {
                        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                        // Collect all the non-layout blocks in the drawing

                        foreach (ObjectId btrId in bt)
                        {
                            var btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
                            if (!btr.IsLayout)
                            {
                                toProcess.Add(btrId);
                            }
                        }
                    }
                    else
                    {
                        // A specific block was selected, let's open it

                        var brId = per.ObjectId;
                        var br = (BlockReference)tr.GetObject(brId, OpenMode.ForRead);

                        // Collect the ID of its underlying block definition

                        toProcess.Add(br.BlockTableRecord);
                    }

                    var brIds = MoveWipeoutsToBottom(tr, toProcess);
                    var count = toProcess.Count;

                    // Open each of the returned block references and
                    // request that they be redrawn

                    foreach (ObjectId brId in brIds)
                    {
                        var br = (BlockReference)tr.GetObject(brId, OpenMode.ForWrite);

                        // We want to redraw a specific block, so let's modify a
                        // property on the selected block reference

                        // We might also have called this method:
                        // br.RecordGraphicsModified(true);
                        // but setting a property works better with undo

                        br.Visible = br.Visible;
                    }

                    // Report the number of blocks modified (after
                    // being filtered by MoveWipeoutsToBottom())

                    ed.WriteMessage("\nModified {0} block definition{1}.", count, count == 1 ? "" : "s");

                    // Commit the transaction

                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                doc.Editor.WriteMessage(
                "\nException: {0}", e.Message
                );
            }
        }

        // Move the wipeouts to the bottom of the specified
        // block definitions

        private static ObjectIdCollection MoveWipeoutsToBottom(Transaction tr, ObjectIdCollection ids)
        {
            // The IDs of any block references we find
            // to return to the call for updating

            var brIds = new ObjectIdCollection();

            // We only need to get this once

            var wc = RXClass.GetClass(typeof(Wipeout));

            // Take a copy of the IDs passed in, as we'll modify the
            // original list for the caller to use

            var btrIds = new ObjectId[ids.Count];
            ids.CopyTo(btrIds, 0);

            // Loop through the blocks passed in, opening each one

            foreach (var btrId in btrIds)
            {
                var btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForWrite);

                // Collect the wipeouts in the block

                var wipeouts = new ObjectIdCollection();
                foreach (ObjectId id in btr)
                {
                    var ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
                    if (ent.GetRXClass().IsDerivedFrom(wc))
                    {
                        wipeouts.Add(id);
                    }
                }

                // Move the collected wipeouts to the bottom

                if (wipeouts.Count > 0)
                {
                    // Modify the draw order table, if we have wipepouts

                    var dot = (DrawOrderTable)tr.GetObject(btr.DrawOrderTableId, OpenMode.ForWrite);
                    dot.MoveToBottom(wipeouts);

                    // Collect the block references to this block, to pass
                    // back to the calling function for updating

                    var btrBrIds = btr.GetBlockReferenceIds(false, false);
                    foreach (ObjectId btrBrId in btrBrIds)
                    {
                        brIds.Add(btrBrId);
                    }
                }
                else
                {
                    ids.Remove(btrId);
                }
            }
            return brIds;
        }
    }
}

