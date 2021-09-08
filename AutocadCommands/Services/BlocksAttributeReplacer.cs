using System.Collections.Generic;
using AutocadCommands.Helpers;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using CommonHelpers;
using TransactionManager = Autodesk.AutoCAD.ApplicationServices.TransactionManager;

namespace AutocadCommands.Services
{
    public class BlocksAttributeReplacer : CommandPrototype
    {
        private string _attributeName;
        private string _attributeValue;
        private PromptSelectionResult _selectedBlocks;


        public BlocksAttributeReplacer(Document doc) : base(doc)
        {
        }

        public override bool Init()
        {
            #region Dialog with user

            var promptResult = _ed.GetString("\nEnter attribute name <CABLEDESIGNATION>: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;

            _attributeName = promptResult.StringResult;
            if (_attributeName == null)
                return false;

            if (_attributeName == string.Empty)
                _attributeName = "CABLEDESIGNATION";

            promptResult = _ed.GetString("\nEnter value: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;

            _attributeValue = promptResult.StringResult;
            if (_attributeValue == null)
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
            

            var attributesDict = new Dictionary<string, string>()
            {
                {_attributeName, _attributeValue}
            };

            // Lock the document
            using var acLckDoc = _doc.LockDocument();

            var objIds = new ObjectIdCollection(_selectedBlocks.Value.GetObjectIds());

            using var tr = _db.TransactionManager.StartTransaction();
            // Test each entity in the container...
            foreach (ObjectId objId in objIds)
            {
                var btr = (BlockReference)tr.GetObject(objId, OpenMode.ForRead);
                if (btr == null) continue;
                var attrColl = btr.AttributeCollection;
                AttributeHelper.SetAttributes(tr, attrColl, attributesDict);

            }
            tr.Commit();
        }

        
    }
}