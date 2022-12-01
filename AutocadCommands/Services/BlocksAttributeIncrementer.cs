using AutocadCommands.Helpers;
using AutocadCommands.Utils;
using AutocadTerminalsManager.Model;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using CommonHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutocadCommands.Services
{
    public class BlocksAttributeIncrementer : CommandPrototype
    {
        private string _attributeName = "CABLEDESIGNATION";
        private string _searchMethod;
        private int _startCounter;
        private PromptSelectionResult _selectedBlocks;
        private string _searchString;

        public BlocksAttributeIncrementer(Document doc) : base(doc)
        {
        }

        public override bool Init()
        {
            #region Dialog with user

            var promptResult = _ed.GetString("\nSearch method [Before After] <After>: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;

            _searchMethod = promptResult.StringResult?.ToUpper();
            if (string.IsNullOrEmpty(_searchMethod))
                _searchMethod = "A";


            promptResult = _ed.GetString("\nWhat are we looking for?: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;

            _searchString = promptResult.StringResult;
            if (_searchString == null)
                return false;

            promptResult = _ed.GetString("\nEnter start increment value: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;

            var startCounter = promptResult.StringResult;
            if (startCounter == null)
                return false;
            if (!int.TryParse(startCounter, out _startCounter))
                return false;


            var filter = new SelectionFilter(
                new[]
                {
                    new TypedValue(0, "INSERT"),
                    new TypedValue(2, "*")
                });

            var opts = new PromptSelectionOptions
            {
                MessageForAdding = "Select block references: "
            };

            //Make the selection   
            _selectedBlocks = _ed.GetSelection(opts, filter);
            return _selectedBlocks.Status == PromptStatus.OK;

            #endregion
        }

        public override void Run()
        {
            // Lock the document
            using var acLckDoc = _doc.LockDocument();
            var objIds = new ObjectIdCollection(_selectedBlocks.Value.GetObjectIds());
            

            var loppedObjects = GetFakeAcadObjectsCollection(objIds);

            var comparer = new BlocksComparer();
            loppedObjects.Sort(comparer);

            ChangeAttributes(loppedObjects);
            SaveChanges(loppedObjects);
        }

        private void SaveChanges(List<AcadObjectWithAttributes> loppedObjects)
        {
            using var acTrans = _db.TransactionManager.StartTransaction();
            foreach (var loppedObject in loppedObjects)
            {
                var blockId = loppedObject.GetId;
                AttributeHelper.SetBlockAttributes(acTrans, blockId, loppedObject.Attributes);
            }

            acTrans.Commit();
        }

        private List<AcadObjectWithAttributes> GetFakeAcadObjectsCollection(ObjectIdCollection objIds)
        {
            var fakeAcadObjects = new List<AcadObjectWithAttributes>();
            using (var acTrans = _db.TransactionManager.StartTransaction())
            {
                

                foreach (ObjectId blkId in objIds)
                {
                    //var attrValue = AttributeHelper.GetAttributeValueFromBlock(acTrans, blkId, _attributeName);
                    var entity = (Entity)acTrans.GetObject(blkId, OpenMode.ForRead);
                    var blockRef = (BlockReference)entity;
                    var attrCol = blockRef.AttributeCollection;

                    fakeAcadObjects.Add(new AcadObjectWithAttributes
                    {
                        Entity = entity,
                        Attributes = AttributeHelper.GetAttributes(acTrans, attrCol),
                        PositionX = blockRef.Position.X,
                        PositionY = blockRef.Position.Y,
                        PositionZ = blockRef.Position.Z
                    });
                }

                acTrans.Commit();
               
            }
            return fakeAcadObjects;
        }

        private void ChangeAttributes(List<AcadObjectWithAttributes> loppedObjects)
        {
            foreach (var loppedObj in loppedObjects)
            {
                if (!loppedObj.Attributes.TryGetValue(_attributeName, out var attrValue))
                    continue;
                var result = FindReplaceWithIncrement(attrValue, _startCounter++);
                loppedObj.Attributes[_attributeName] = result;
            }
        }

        public string FindReplaceWithIncrement(string attrValue, int counter)
        {
            var result = attrValue;
            
            if (!attrValue.Contains(_searchString))
                return null;
            switch (_searchMethod)
            {
                case "A":
                    {
                        result = attrValue.Substring(0, attrValue.LastIndexOf(_searchString) + _searchString.Length);
                        result += counter;
                        
                        break;
                    }
                case "B":
                    {
                        result = attrValue.Substring(attrValue.IndexOf(_searchString));
                        result = counter + result;
                       
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return result;
        }
    }
}
