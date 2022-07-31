using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using CommonHelpers.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommonHelpers
{
    public static class BlockUtils
    {

        public static ObjectId InsertBlockFormFile(Database db, string blockName, Point3d point3D, IEnumerable<FakeAttribute> attributes, string layer)
        {
            using Transaction tr = db.TransactionManager.StartTransaction();
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

            var objId = BlockUtils.GetBlockId(bt, blockName);

            var curSpace = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);

            var br1 = (BlockTableRecord)tr.GetObject(bt[blockName], OpenMode.ForRead);
            /*foreach (ObjectId id in br1)
            {
                var ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
                if (ent.Layer == "0")
                {
                    ent.UpgradeOpen();
                    ent.Layer = layer;
                }    
            }
            */
            var br = curSpace.InsertBlockReference(blockName, point3D, attributes, layer);
            //wire.PointConnectedToMultiWire
            //_db.TransactionManager.QueueForGraphicsFlush();

            tr.Commit();

            return br.Id;
        }

        public static ObjectId GetBlockId(this BlockTable blockTable, string blockName)
        {
            if (blockTable == null)
                throw new ArgumentNullException("blockTable");

            var db = blockTable.Database;
            if (blockTable.Has(blockName))
                return blockTable[blockName];

            try
            {
                string ext = Path.GetExtension(blockName);
                if (ext == "")
                    blockName += ".dwg";
                string blockPath;
                if (File.Exists(blockName))
                    blockPath = blockName;
                else
                    blockPath = HostApplicationServices.Current.FindFile(blockName, db, FindFileHint.Default);

                blockTable.UpgradeOpen();
                using (Database tmpDb = new(false, true))
                {
                    tmpDb.ReadDwgFile(blockPath, FileShare.Read, true, null);
                    return blockTable.Database.Insert(Path.GetFileNameWithoutExtension(blockName), tmpDb, true);
                }
            }
            catch
            {
                return ObjectId.Null;
            }
        }

        public static BlockReference InsertBlockReference(this BlockTableRecord target, string blkName, Point3d insertPoint, IEnumerable<FakeAttribute> attributes, string layer)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            var db = target.Database;
            var tr = db.TransactionManager.TopTransaction;
            if (tr == null)
                throw new ArgumentNullException("transaction");//AcRx.Exception(ErrorStatus.NoActiveTransactions);

            BlockReference br = null;
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

            ObjectId btrId = bt.GetBlockId(blkName);

            if (btrId != ObjectId.Null)
            {
                br = new BlockReference(insertPoint, btrId)
                {
                    Layer = layer
                };
                //var btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
                target.AppendEntity(br);

                tr.AddNewlyCreatedDBObject(br, true);

                br.AddAttributeReferences(attributes);
            }
            return br;
        }

        public static void AddAttributeReferences(this BlockReference target, IEnumerable<FakeAttribute> attributes)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            var tr = target.Database.TransactionManager.TopTransaction;
            if (tr == null)
                throw new ArgumentNullException("transaction");//AcRx.Exception(ErrorStatus.NoActiveTransactions);

            if (attributes == null)
                return;

            var btr = (BlockTableRecord)tr.GetObject(target.BlockTableRecord, OpenMode.ForRead);
            var attDefClass = RXClass.GetClass(typeof(AttributeDefinition));

            foreach (ObjectId id in btr)
            {
                if (id.ObjectClass != attDefClass)
                    continue;

                var attDef = (AttributeDefinition)tr.GetObject(id, OpenMode.ForRead);
                var attRef = new AttributeReference();
                attRef.SetAttributeFromBlock(attDef, target.BlockTransform);

                foreach (FakeAttribute attribute in attributes)
                {
                    if (attribute.Tag.ToUpper().Equals(attDef.Tag.ToUpper()))
                    {
                        attRef.TextString = attribute.Value;
                        attRef.Layer = attribute.Layer;
                    }
                }
                target.AttributeCollection.AppendAttribute(attRef);
                tr.AddNewlyCreatedDBObject(attRef, true);
            }
        }

        public static void MoveBlockReference(Database db, ObjectId objectId, Vector3d vector)
        {
            var tr = db.TransactionManager.StartTransaction();
            var entity = (Entity)tr.GetObject(objectId, OpenMode.ForWrite);
            var displVector = Matrix3d.Displacement(vector);
            entity.TransformBy(displVector);
            tr.Commit();
        }
    }
}
