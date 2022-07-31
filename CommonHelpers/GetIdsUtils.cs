using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers
{
    public static class GetIdsUtils
    {
        public static IEnumerable<ObjectId> GetIdsByType<T>(Database db, string layer)
        {
            using var tr = db.TransactionManager.StartTransaction();
            var btl = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
            foreach (var item in btl)
            {
                var bk = (BlockTableRecord)tr.GetObject(item, OpenMode.ForRead);

                if (!bk.IsLayout)
                    continue;

                foreach (var obj in bk)
                {
                    Entity entity = (Entity)tr.GetObject(obj, OpenMode.ForWrite);

                    if (entity == null || entity.GetType() != typeof(T))
                        continue;

                    //var br = (T)tr.GetObject(obj, OpenMode.ForRead);
                    if(!entity.Layer.Equals(layer))
                        continue;

                    yield return entity.Id;
                }
            }
            tr.Commit();
        }

        public static IEnumerable<ObjectId> GetBlockRefsByNames(Database db, IEnumerable<string> names)
        {
            using var tr = db.TransactionManager.StartTransaction();
            var btl = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
            foreach (var item in btl)
            {
                var bk = (BlockTableRecord)tr.GetObject(item, OpenMode.ForRead);

                if (!bk.IsLayout)
                    continue;

                foreach (var obj in bk)
                {
                    Entity entity = (Entity)tr.GetObject(obj, OpenMode.ForWrite);

                    if (entity == null || entity.GetType() != typeof(BlockReference))
                        continue;

                    var br = (BlockReference)entity;

                    foreach (var name in names)
                    {
                        if (br.Name.Equals(name))
                            yield return entity.Id;
                    }
                    
                }
            }
            tr.Commit();
        }
    }
}
