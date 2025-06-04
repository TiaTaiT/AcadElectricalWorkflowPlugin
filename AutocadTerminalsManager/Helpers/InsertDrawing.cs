using AutocadCommands.Helpers;
using AutocadTerminalsManager.Model;
using AutocadTerminalsManager.Services;
using CommonHelpers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AutocadTerminalsManager.Helpers
{
    public class InsertDrawing
    {
        private const string _cableDesignation = "CABLEDESIGNATION";
        private const string _cableBrand = "CABLEBRAND";        

        private string _sourceFile;
        private bool _isSurgeProtection;
        private bool _isExplosionProof;
        private IEnumerable<Cable> _cables;
        private Document _doc;
        private Database _currentDb;
        private Editor _editor;
        private Database _sourceDb;
        private bool _orthoMode;

        public InsertDrawing(string sourceFile, Assembly assembly)
        {
            _sourceFile = sourceFile;
            _isSurgeProtection = assembly.IsSourgeProtection;
            _isExplosionProof = assembly.IsExplosionProof;
            _cables = assembly.PerimeterCables;


            // Get the current document and database
            _doc = Application.DocumentManager.MdiActiveDocument;
            _currentDb = _doc.Database;
            _editor = Application.DocumentManager.MdiActiveDocument.Editor;
            _sourceDb = new Database(false, true);

            _orthoMode = _doc.Database.Orthomode;

        }

        /// <summary>
        /// This method is simple analog "Insert" Autocad function
        /// </summary>
        /// <param name="sourceFile">Absolute DWG file path</param>
        public ObjectIdCollection GetSourceDrawingIds()
        {
            // Lock the current document
            using var acLckDocCur = _doc.LockDocument();
            try
            {
                _sourceDb.ReadDwgFile(_sourceFile, FileShare.Read, true, "");
                var acObjIdColl = GetObjectsIdsFromDb(_sourceDb);

                return acObjIdColl;
            }
            catch (System.Exception ex)
            {
                _editor.WriteMessage("\nError during copy: " + ex.Message);
                return null;
            }

            // Unlock the document
        }

        public void PutToTargetDb(ObjectIdCollection objIdColl)
        {
            // Lock the new document
            using var lockDocument = _doc.LockDocument();

            _doc.Database.Orthomode = false;

            // Start a transaction in the new database
            using var acTrans = _currentDb.TransactionManager.StartTransaction();

            // Open the Block table for read
            var blkTblNewDoc = (BlockTable)acTrans.GetObject(
                _currentDb.BlockTableId, OpenMode.ForRead);

            // Open the Block table record Model space for read
            var acBlkTblRecNewDoc = (BlockTableRecord)acTrans.GetObject(
                blkTblNewDoc[BlockTableRecord.ModelSpace], OpenMode.ForRead);

            acTrans.TransactionManager.QueueForGraphicsFlush();

            // Clone the objects to the new database
            var acIdMap = new IdMapping();


            var destOwnerId = acBlkTblRecNewDoc.ObjectId;

            _sourceDb.WblockCloneObjects(objIdColl, destOwnerId, acIdMap, DuplicateRecordCloning.Ignore, false);

            /*
            using (Transaction currDbTr = _currentDb.TransactionManager.StartTransaction())
            {
                
                foreach(ObjectId sourceBlockId in objIdColl)
                {
                    string blockName = GetBlockName(_sourceDb, sourceBlockId);
                    ObjectId targetBlockId = ObjectId.Null;
                    BlockTable b = (BlockTable)acTrans.GetObject(_currentDb.BlockTableId, OpenMode.ForRead);
                    if (b.Has(blockName))
                    {
                        targetBlockId = b[blockName];
                    }

                    SetBlockDrawOrder(sourceBlockId, targetBlockId, acIdMap);
                }

                currDbTr.Commit();

            };
               
            */

            if (!StartJig(acIdMap)) return;

            acTrans.Commit();

            _sourceDb.Dispose();
            _doc.Database.Orthomode = _orthoMode;
            // Unlock the document
        }

        private string GetBlockName(Database sourceDb, ObjectId id)
        {
            var blockName = "";
            using var tr = sourceDb.TransactionManager.StartTransaction();
            var sourceBtr = tr.GetObject(id, OpenMode.ForRead);
            if (sourceBtr != null && sourceBtr is BlockTableRecord)
            {
                blockName = ((BlockTableRecord)sourceBtr).Name;
            }
            return blockName;
        }

        public void SetBlockDrawOrder(ObjectId sourceBlockId, ObjectId targetBlockId, IdMapping iMap)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction t = db.TransactionManager.StartTransaction())
            {
                var sourceBTR = (BlockTableRecord)t.GetObject(sourceBlockId, OpenMode.ForRead);
                var dotSource = (DrawOrderTable)t.GetObject(sourceBTR.DrawOrderTableId, OpenMode.ForRead, true);

                ObjectIdCollection srcDotIds = new ObjectIdCollection();
                srcDotIds = dotSource.GetFullDrawOrder(0);

                var targetBTR = (BlockTableRecord)t.GetObject(targetBlockId, OpenMode.ForRead);
                var dotTarget = (DrawOrderTable)t.GetObject(targetBTR.DrawOrderTableId, OpenMode.ForWrite, true);

                ObjectIdCollection trgDotIds = new ObjectIdCollection();

                foreach (ObjectId oId in srcDotIds)
                {
                    if (iMap.Contains(oId))
                    {
                        IdPair idPair = iMap.Lookup(oId);
                        trgDotIds.Add(idPair.Value);
                    }
                }
                dotTarget.SetRelativeDrawOrder(trgDotIds);
                t.Commit();
            }
        }

        private ObjectId GetTargetId(BlockTable targetBt, string blockName)
        {
            if (targetBt.Has(blockName))
            {
                return targetBt[blockName];
            }
            return new ObjectId();
        }



        /// <summary>
        /// Prepare and start jigging
        /// </summary>
        /// <param name="acIdMap"></param>
        /// <returns>Return false if user canceled jigging</returns>
        private bool StartJig(IdMapping acIdMap)
        {
            using var tr = _currentDb.TransactionManager.StartTransaction();

            var onlyPrimaryEntities = GetPrimaryEntities(tr, acIdMap);

            //Objects with right attributes
            var attrObjects =
                AttributeHelper.GetObjectsWithAttribute(tr, onlyPrimaryEntities, "CABLEDESIGNATION");

            //Replace attributes in fake objects
            ReplaceAttributes(attrObjects, _cables);

            foreach (var obj in attrObjects)
            {
                var br = (BlockReference)obj.Entity;
                AttributeHelper.SetAttributes(tr, br.AttributeCollection, obj.Attributes);
            }

            //Replace attributes in fake objects
            if (_isSurgeProtection)
            {
                //Objects with right attributes
                var terminalObjects =
                    AttributeHelper.GetObjectsWithAttribute(tr, onlyPrimaryEntities, SharedStrings.TerminalSign);

                ReplaceTerminalsAttributes(terminalObjects);
                
                foreach (var terminal in terminalObjects)
                {
                    var br = (BlockReference)terminal.Entity;
                    AttributeHelper.SetAttributes(tr, br.AttributeCollection, terminal.Attributes);
                }
            }
            

            var jig = new DragEntitiesJig(onlyPrimaryEntities, new Point3d(0, 0, 0));
            var jigRes = _doc.Editor.Drag(jig);

            if (jigRes.Status != PromptStatus.OK) return false;

            jig.TransformEntities();

            tr.Commit();
            return true;
        }

        private void ReplaceTerminalsAttributes(IEnumerable<AcadObjectWithAttributes> terminalObjects)
        {
            foreach(var terminal in terminalObjects)
            {
                if(terminal.Attributes.TryGetValue(SharedStrings.DescriptionTag1, out var descValue))
                {
                    var strWithProtection =
                        SharedStrings.SurgeProtectionCharacters[0] + 
                        descValue + 
                        SharedStrings.SurgeProtectionCharacters[1];

                    terminal.Attributes[SharedStrings.DescriptionTag1] = strWithProtection;
                }
            }
        }

        private ObjectIdCollection GetPrimaryIds(IEnumerable idMap)
        {
            var ids = new ObjectIdCollection();
            foreach (IdPair idPair in idMap)
            {
                if (idPair.IsPrimary)
                    ids.Add(idPair.Value);
            }

            return ids;
        }

        private IEnumerable<Entity> GetPrimaryEntities(Transaction tr, IEnumerable idMap)
        {
            var entities = new List<Entity>();
            var ids = GetPrimaryIds(idMap);
            foreach (ObjectId id in ids)
            {
                if (tr.GetObject(id, OpenMode.ForWrite) is not Entity entity) continue;
                entities.Add(entity);
            }

            return entities;
        }

        private ObjectIdCollection GetObjectsIdsFromDb(Database sourceDb)
        {
            using var tr = sourceDb.TransactionManager.StartTransaction();
            // получаем ссылку на пространство модели (ModelSpace)
            // открываем таблицу блоков документа
            var blkTbl = tr.GetObject(sourceDb.BlockTableId, OpenMode.ForRead) as BlockTable;

            // открываем пространство модели (Model Space) - оно является одной из записей в таблице блоков документа
            var ms = tr.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            //get the draw order table of the block

            var drawOrder = (DrawOrderTable)tr.GetObject(ms.DrawOrderTableId, OpenMode.ForWrite);

            var orderingColl = drawOrder.GetFullDrawOrder(0);

            //_drawOrder.SetRelativeDrawOrder(ms1);

            var acObjIdColl = new ObjectIdCollection();

            // "пробегаем" по всем объектам в пространстве модели
            /*
            foreach (var id in ms)
            {
                if (id.IsErased) continue;
                acObjIdColl.Add(id);
            }
            */
            foreach (ObjectId id in orderingColl)
            {
                if (id.IsErased) continue;
                var obj = tr.GetObject(id, OpenMode.ForWrite);
                if (obj.GetType() == typeof(BlockReference))
                {
                    var blockRef = (BlockReference)obj;
                    var btr = (BlockTableRecord)tr.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead);

                    var dot = (DrawOrderTable)tr.GetObject(btr.DrawOrderTableId, OpenMode.ForWrite);
                    _editor.WriteMessage("\n" + btr.Name);
                    var oColl = dot.GetFullDrawOrder(0);

                    foreach (var o in btr)
                    {

                        _editor.WriteMessage("\n" + o.ToString());
                    }

                }
                acObjIdColl.Add(id);
            }

            tr.Commit();
            return acObjIdColl;
        }



        /// <summary>
        /// The method replaces fields "CABLEDESIGNATION" and "CABLEBRAND" in fake objects.
        /// To do this, in the source drawing, the "CABLEDESIGNATION" field of each cable must be numbered starting from 1 (1,2,3,...).
        /// All fields with a number are replaced with a cable designation in the order in which they appear in the json-file (cables parameter). 
        /// </summary>
        /// <param name="attrObjects">List of fake objects with copy of all attributes and link to original entities</param>
        /// <param name="cables">List of cables from json file</param>
        private static void ReplaceAttributes(IEnumerable<AcadObjectWithAttributes> attrObjects,
            IEnumerable<Cable> cables)
        {
            Debug.WriteLine("-----------------------------------");

            foreach (var attrObject in attrObjects)
            {
                var attributes = attrObject.Attributes;


                if (!attributes.TryGetValue(_cableDesignation, out var cableIndexStr))
                    continue;

                if (!int.TryParse(cableIndexStr, out var cableIndex))
                    continue;

                // The numbering of cables in the drawing starts from one and not from zero
                cableIndex -= 1;

                if ((cableIndex) > cables.Count())
                {
                    Application.ShowAlertDialog("Variable \"cableIndex\" out of index. Cables.Count() = " +
                                                cables.Count() + "; cableIndex = " + cableIndex);
                    return;
                }

                attributes[_cableDesignation] = cables.ElementAt(cableIndex).Designation;


                if (!attributes.ContainsKey(_cableBrand))
                    continue;

                attributes[_cableBrand] = cables.ElementAt(cableIndex).Brand;
            }
        }
    }
}