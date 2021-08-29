using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace CommonHelpers
{
	public static class AttributesHelper2
    {
		public static void UpdateAttributesByBlockName(Database db, string blockName, string attbName, string attbValue)
		{
			var doc = Application.DocumentManager.MdiActiveDocument;
			var ed = doc.Editor;
			// Get the IDs of the spaces we want to process
			// and simply call a function to process each
			ObjectId msId, psId;
			var tr = db.TransactionManager.StartTransaction();
			using (tr)
			{
				var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
				msId = bt[BlockTableRecord.ModelSpace];
				psId = bt[BlockTableRecord.PaperSpace];
				// Not needed, but quicker than aborting
				tr.Commit();
			}
			UpdateAttributesInBlock(msId, blockName, attbName, attbValue);
			UpdateAttributesInBlock(psId, blockName, attbName, attbValue);
			//ed.Regen();
			// Display the results
			/*
			ed.WriteMessage("\nProcessing file: " + db.Filename);
			ed.WriteMessage("\nUpdated {0} instance{1} of " + "attribute {2} in the modelspace.", msCount, msCount == 1 ? "" : "s", attbName);
			ed.WriteMessage("\nUpdated {0} instance{1} of " + "attribute {2} in the default paperspace.", psCount, psCount == 1 ? "" : "s", attbName);
			*/
		}

		private static void UpdateAttributesInBlock(ObjectId btrId, string blockName, string attbName, string attbValue)
		{
			// Will return the number of attributes modified
			var changedCount = 0;
			var doc = Application.DocumentManager.MdiActiveDocument;
             
            using var tr = doc.TransactionManager.StartTransaction();
            
            var btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
            // Test each entity in the container...
            foreach (ObjectId entId in btr)
            {
                var ent = tr.GetObject(entId, OpenMode.ForRead) as Entity;
                if (ent == null) continue;
                var br = ent as BlockReference;
                if (br == null) continue;
                var bd = (BlockTableRecord)tr.GetObject(br.BlockTableRecord, OpenMode.ForRead);
                // ... to see whether it's a block with
                // the name we're after
                if (bd.Name.ToUpper() != blockName) continue;
                // Check each of the attributes...
                foreach (ObjectId arId in br.AttributeCollection)
                {
                    var obj = tr.GetObject(arId, OpenMode.ForRead);
                    var ar = obj as AttributeReference;
                    if (ar == null) continue;
                    // ... to see whether it has
                    // the tag we're after
                    if (ar.Tag.ToUpper() != attbName) continue;
                    // If so, update the value
                    // and increment the counter
                    ar.UpgradeOpen();
                    ar.TextString = attbValue;
                    ar.DowngradeOpen();
                    changedCount++;
                }
                // Recurse for nested blocks
                //changedCount += UpdateAttributesInBlock(br.BlockTableRecord, blockName, attbName, attbValue);
            }
            tr.Commit();
            //return changedCount;
		}
	}
}