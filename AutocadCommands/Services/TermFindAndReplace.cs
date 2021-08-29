using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AutocadCommands.Models;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace AutocadCommands.Services
{
    public class TermFindAndReplace
    {
        private readonly Database _db;
        private readonly Document _doc;
        private readonly Editor _ed;

        public TermFindAndReplace(Editor ed, Document doc, Database db)
        {
            _ed = ed;
            _doc = doc;
            _db = db;
        }

        public void Run()
        {
            #region Dialog with user
            
            var promptResult = _ed.GetString("\nEnter a sequence of characters to replace: ");
            if (promptResult.Status != PromptStatus.OK)
                return;

            var searchString = promptResult.StringResult;
            if (searchString == null)
                return;

            promptResult = _ed.GetString("\nReplaced by: ");
            if (promptResult.Status != PromptStatus.OK)
                return;

            var replaceString = promptResult.StringResult;
            if (replaceString == null)
                return;

            promptResult = _ed.GetString("\nSearch method [First End] <Any>: ");
            if (promptResult.Status != PromptStatus.OK)
                return;

            var searchMethod = promptResult.StringResult?.ToUpper();
            if (searchMethod == null)
                return;
            
            

            var filter = new SelectionFilter(
                new[]
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

            #endregion

            var terminals = new List<Terminal>();

            // Lock the document
            using var acLckDoc = _doc.LockDocument();
            var objIds = new ObjectIdCollection(res.Value.GetObjectIds());

            using (var acTrans = _db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId blkId in objIds)
                {
                    terminals = TerminalsHelper.GetTerminals(acTrans, objIds, false);
                }

                acTrans.Commit();
            }

            using (var acTrans = _db.TransactionManager.StartTransaction())
            {
                FindAndReplace(terminals, searchString, replaceString, searchMethod);
                TerminalsHelper.SetTerminals(acTrans, objIds, terminals);
                acTrans.Commit();
            }
        }

        private void FindAndReplace(IEnumerable<Terminal> terminals, string searchString, string replaceString, string searchMethod)
        {
            foreach (var terminal in terminals)
            {
                var desc1 = terminal.Description1;
                if (searchMethod.Equals("F"))
                {
                    if (desc1.StartsWith(searchString))
                    {
                        desc1 = replaceString + desc1.Substring(searchString.Length);
                    }
                }
                else if (searchMethod.Equals("E"))
                {
                    if (desc1.EndsWith(searchString))
                    {
                        desc1 = desc1.Substring(0, desc1.Length - searchString.Length) + replaceString;
                    }
                }
                else
                {
                    if (desc1.Contains(searchString))
                    {
                        desc1 = desc1.Replace(searchString, replaceString);
                    }
                }

                terminal.Description1 = desc1;
            }
        }
    }
}