using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Exception = System.Exception;

namespace AutocadCommands.Services
{
    public static class BlockHelper
    {
        public static ObjectId InsertElectricalBlock(Point3d insPt, string blockName)
        {
            var blockId = ObjectId.Null;

            using var resBuf = new ResultBuffer
            {
                new TypedValue((int) LispDataType.Text, "c:wd_insym2"), // Lisp fucntion to call
                new TypedValue((int) LispDataType.Text, blockName),// Block name
                new TypedValue((int) LispDataType.Point3d, insPt),// Insert point
                new TypedValue((int) LispDataType.Nil),// Scale
                new TypedValue((int) LispDataType.Nil)// Options
            };

            using var res = Application.Invoke(resBuf);
            if (res == null)
                return blockId;

            foreach (var val in res)
            {
                if (val.TypeCode == (int)LispDataType.ObjectId)
                {
                    blockId = (ObjectId)val.Value;
                }
            }

            return blockId;
        }

        public static void InsertBlock(Database db, Point3d insPt, string blockName)
        {
            using var tr = db.TransactionManager.StartTransaction();
            // check if the block table already has the 'blockName'" block
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            if (!bt.Has(blockName))
            {
                try
                {
                    // search for a dwg file named 'blockName' in AutoCAD search paths
                    var filename = HostApplicationServices.Current.FindFile(blockName + ".dwg", db, FindFileHint.Default);
                        
                    // add the dwg model space as 'blockName' block definition in the current database block table
                    using var sourceDb = new Database(false, true);
                    sourceDb.ReadDwgFile(filename, FileOpenMode.OpenForReadAndAllShare, true, "");
                    db.Insert(blockName, sourceDb, true);
                }
                catch
                {
                    //_ed.WriteMessage($"\nBlock '{blockName}' not found.");
                    return;
                }
            }

            // create a new block reference
            else
            {
                //Also open modelspace - we'll be adding our BlockReference to it
                var ms = bt[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForWrite) as BlockTableRecord;

                using var br = new BlockReference(insPt, bt[blockName]);
                //Add the block reference to modelspace
                ms.AppendEntity(br);
                tr.AddNewlyCreatedDBObject(br, true);

                var blockDef = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                //Iterate block definition to find all non-constant
                // AttributeDefinitions
                foreach (var id in blockDef)
                {
                    DBObject obj = id.GetObject(OpenMode.ForRead);
                    AttributeDefinition attDef = obj as AttributeDefinition;
                    if ((attDef == null) || (attDef.Constant)) continue;

                    //This is a non-constant AttributeDefinition
                    //Create a new AttributeReference
                    using AttributeReference attRef = new AttributeReference();
                            
                    attRef.SetAttributeFromBlock(attDef, br.BlockTransform);
                    attRef.TextString = "Hello World";
                    //Add the AttributeReference to the BlockReference
                    br.AttributeCollection.AppendAttribute(attRef);
                    tr.AddNewlyCreatedDBObject(attRef, true);
                }
                /*
                        space.AppendEntity(br);
                        tr.AddNewlyCreatedDBObject(br, true);
                        */
            }

            tr.Commit();
        }

        private static void ReplaceBlock(Database db, Transaction tr, string oldName, string newName)
        {
            //using (var tr = db.TransactionManager.StartTransaction())
            //{
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

            // check if the block table contains old block
            if (!bt.Has(oldName))
            {
                Application.ShowAlertDialog(oldName + " not found");
                return;
            }
            var oldBlockId = bt[oldName];

            // check if the block table contains onew block
            ObjectId newBlockId;
            if (bt.Has(newName))
            {
                newBlockId = bt[newName];
            }
            // try to insert new bloc from a DWG file in search paths
            else
            {
                try
                {
                    var filename = HostApplicationServices.Current.FindFile(newName + ".dwg", db, FindFileHint.Default);
                    using (var sourceDb = new Database(false, true))
                    {
                        sourceDb.ReadDwgFile(filename, FileOpenMode.OpenForReadAndAllShare, true, null);
                        newBlockId = db.Insert(newName, sourceDb, true);
                    }
                }
                catch (System.Exception)
                {
                    Application.ShowAlertDialog(newName + " not found");
                    return;
                }
            }

            // replace all references to old block by references to new block
            var oldBtr = (BlockTableRecord)tr.GetObject(oldBlockId, OpenMode.ForRead);
            foreach (ObjectId id in oldBtr.GetBlockReferenceIds(true, true))
            {
                var br = (BlockReference)tr.GetObject(id, OpenMode.ForWrite);
                br.BlockTableRecord = newBlockId;
            }
        }
    }
}