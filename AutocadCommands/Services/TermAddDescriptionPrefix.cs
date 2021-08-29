using System;
using System.Collections.Generic;
using AutocadCommands.Models;
using AutocadCommands.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace AutocadCommands.Services
{
    public class TermAddDescriptionPrefix
    {
        private readonly Editor _ed;
        private readonly Document _doc;
        private readonly Database _db;

        public TermAddDescriptionPrefix(Editor ed, Document doc, Database db)
        {
            _ed = ed;
            _doc = doc;
            _db = db;
        }

        public void Run()
        {
            var promptResult = _ed.GetString("\nEnter the prefix character sequence: ");
            if (promptResult.Status != PromptStatus.OK)
                return;

            var prefix = promptResult.StringResult;
            if (prefix == null)
                return;

            var filter = new SelectionFilter(new[]
                {
                    new TypedValue(0, "INSERT"), new TypedValue(2, "*T0002_*")
                });

            var opts = new PromptSelectionOptions
            {
                MessageForAdding = "Select block references: "
            };

            //Make the selection   
            PromptSelectionResult res = _ed.GetSelection(opts, filter);
            if (res.Status != PromptStatus.OK)
                return;
            
            var terminals = new List<Terminal>();

            // Lock the document
            using (DocumentLock acLckDoc = _doc.LockDocument())
            {
                var objIds = new ObjectIdCollection(res.Value.GetObjectIds());

                using (Transaction acTrans = _db.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId blkId in objIds)
                    {
                        terminals = TerminalsHelper.GetTerminals(acTrans, objIds, false);
                    }

                    acTrans.Commit();
                }

                using (Transaction acTrans = _db.TransactionManager.StartTransaction())
                {
                    IComparer<Terminal> comparer = new TerminalsComparer();
                    terminals.Sort(comparer);
                    AddPrefix(terminals, prefix);
                    TerminalsHelper.SetTerminals(acTrans, objIds, terminals);
                    acTrans.Commit();
                }
            }
        }

        private void AddPrefix(List<Terminal> terminals, string prefix)
        {
            foreach (var terminal in terminals)
            {
                terminal.Description1 = prefix + terminal.Description1;
            }
        }
    }
}