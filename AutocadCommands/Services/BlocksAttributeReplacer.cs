using System.Collections.Generic;
using AutocadCommands.Helpers;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using TransactionManager = Autodesk.AutoCAD.ApplicationServices.TransactionManager;

namespace AutocadCommands.Services
{
    public class BlocksAttributeReplacer
    {
        private readonly Editor _ed;
        private readonly Document _doc;
        private readonly Database _db;

        public BlocksAttributeReplacer(Editor ed, Document doc, Database db)
        {
            _ed = ed;
            _doc = doc;
            _db = db;
        }

        public void Run()
        {
            #region Dialog with user

            var promptResult = _ed.GetString("\nEnter attribute name <CABLEDESIGNATION>: ");
            if (promptResult.Status != PromptStatus.OK)
                return;

            var attributeName = promptResult.StringResult;
            if (attributeName == null)
                return;

            promptResult = _ed.GetString("\nEnter value: ");
            if (promptResult.Status != PromptStatus.OK)
                return;

            var attributeValue = promptResult.StringResult;
            if (attributeValue == null)
                return;



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
            var res = _ed.GetSelection(opts, filter);
            if (res.Status != PromptStatus.OK)
                return;

            #endregion

            var attributesDict = new Dictionary<string, string>()
            {
                {attributeName, attributeValue}
            };

            // Lock the document
            using var acLckDoc = _doc.LockDocument();

            var objIds = new ObjectIdCollection(res.Value.GetObjectIds());

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