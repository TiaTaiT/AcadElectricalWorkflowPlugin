using System.Collections.Generic;

namespace CommonHelpers
{
    public static class GetObjectsUtils
    {
        public static List<T> GetObjects<T>(Database db, Transaction tr, string layer) where T : Entity
        {
            var result = new List<T>();
            var btl = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

            foreach (var item in btl)
            {
                var bk = (BlockTableRecord)tr.GetObject(item, OpenMode.ForRead);

                if (!bk.IsLayout)
                    continue;

                foreach (var obj in bk)
                {
                    var entity = tr.GetObject(obj, OpenMode.ForRead) as Entity;

                    if (entity is T typedEntity && entity.Layer == layer)
                    {
                        result.Add(typedEntity);
                    }
                }
            }

            return result;
        }


        public static IEnumerable<ObjectId> GetBlockIdsByNames(Database db, Transaction tr, IEnumerable<string> names)
        {
            var result = new List<ObjectId>();
            var btl = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
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
                            //yield return entity.Id;
                            result.Add(entity.Id);
                    }

                }
            }
            return result;
        }
    }
}
