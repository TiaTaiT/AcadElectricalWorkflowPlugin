﻿using System.Collections.Generic;

using Teigha.DatabaseServices;


namespace CommonHelpers
{
    public static class GetObjectsUtils
    {
        public static IEnumerable<T> GetObjects<T>(Database db, string layer) where T : Entity
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
                    var entity = (Entity)tr.GetObject(obj, OpenMode.ForRead);

                    if (entity == null || entity.GetType() != typeof(T))
                        continue;

                    //var br = (T)tr.GetObject(obj, OpenMode.ForRead);
                    if (!entity.Layer.Equals(layer))
                        continue;

                    yield return (T)entity;
                }
            }
            tr.Commit();
        }

        public static IEnumerable<ObjectId> GetBlockIdsByNames(Database db, IEnumerable<string> names)
        {
            var result = new List<ObjectId>();
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
                            //yield return entity.Id;
                            result.Add(entity.Id);
                    }

                }
            }
            tr.Commit();
            return result;
        }
    }
}
