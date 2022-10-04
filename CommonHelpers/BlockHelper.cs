using AutocadCommands.Helpers;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Collections;
using System.Collections.Generic;
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
                new TypedValue((int) LispDataType.Text, "(c:wd_insym2)"), // Lisp fucntion to call
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

        public static IEnumerable<ObjectId> EraseBlockIfExist(Database db, Transaction tr, Point3d point3D)
        {
            //var result = false;
            //using (var tr = db.TransactionManager.StartTransaction())
            //{
            var erasedIds = new List<ObjectId>();

            var btl = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
            foreach (var item in btl)
            {
                var bk = (BlockTableRecord)tr.GetObject(item, OpenMode.ForRead);
                if (bk.IsLayout)
                {
                    foreach (var obj in bk)
                    {
                        Entity ent = (Entity)tr.GetObject(obj, OpenMode.ForWrite);
                        if (ent != null && ent.GetType() == typeof(BlockReference))
                        {
                            var br = (BlockReference)tr.GetObject(obj, OpenMode.ForRead);
                            if (br.Position.IsEqualTo(point3D))
                            {
                                ent.Erase();
                                //result = true;
                                erasedIds.Add(ent.ObjectId);
                            }
                        }
                    }
                }
            }
            //tr.Commit();
            //}
            return erasedIds;
        }

        public static IEnumerable<ObjectId> EraseBlocksWithAttribute(Database db, Transaction tr, string tagName, string tagValue)
        {
            var erasedIds = new List<ObjectId>();
            var objectIds = AttributeHelper.GetObjectsWithAttribute(db, tr, tagName, tagValue);
            try
            {
                foreach (var id in objectIds)
                {
                    var entity = tr.GetObject(id, OpenMode.ForWrite);
                    entity.Erase();
                    
                    erasedIds.Add(entity.ObjectId);
                }
            }
            catch
            {
            }
            return erasedIds;
        }

        public static void EraseEntitiesByIds(Database db, IEnumerable<ObjectId> blockIds)
        {

            using Transaction tr = db.TransactionManager.StartTransaction();
            foreach (var id in blockIds)
            {
                var entity = (Entity)tr.GetObject(id, OpenMode.ForWrite, false, true);
                entity?.Erase();
                entity?.Dispose();
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