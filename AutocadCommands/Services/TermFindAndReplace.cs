using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AutocadCommands.Models;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using CommonHelpers;

namespace AutocadCommands.Services
{
    public class TermFindAndReplace : CommandPrototype
    {
        private string _searchString;
        private string replaceString;
        private string _searchMethod;
        private PromptSelectionResult _selectedBlocks;

        public TermFindAndReplace(Document doc) : base(doc)
        {
        }

        public override bool Init()
        {
            #region Dialog with user

            var promptResult = _ed.GetString("\nEnter a sequence of characters to replace: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;

            _searchString = promptResult.StringResult;
            if (_searchString == null)
                return false;

            promptResult = _ed.GetString("\nReplaced by: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;

            replaceString = promptResult.StringResult;
            if (replaceString == null)
                return false;

            promptResult = _ed.GetString("\nSearch method [First End] <Any>: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;

            _searchMethod = promptResult.StringResult?.ToUpper();
            if (_searchMethod == null)
                return false;



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
            _selectedBlocks = _ed.GetSelection(opts, filter);
            return _selectedBlocks.Status == PromptStatus.OK;

            #endregion
        }

        public override void Run()
        {
            

            var terminals = new List<Terminal>();

            // Lock the document
            using var acLckDoc = _doc.LockDocument();
            var objIds = new ObjectIdCollection(_selectedBlocks.Value.GetObjectIds());

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
                FindAndReplace(terminals, _searchString, replaceString, _searchMethod);
                TerminalsHelper.SetTerminals(acTrans, objIds, terminals);
                acTrans.Commit();
            }
        }

        private void FindAndReplace(IEnumerable<Terminal> terminals, string searchString, string replaceString, string searchMethod)
        {
            foreach (var terminal in terminals)
            {
                var desc1 = terminal.Description1;
                switch (searchMethod)
                {
                    case "F":
                    {
                        if (desc1.StartsWith(searchString))
                        {
                            desc1 = replaceString + desc1.Substring(searchString.Length);
                        }

                        break;
                    }
                    case "E":
                    {
                        if (desc1.EndsWith(searchString))
                        {
                            desc1 = desc1.Substring(0, desc1.Length - searchString.Length) + replaceString;
                        }

                        break;
                    }
                    default:
                    {
                        if (desc1.Contains(searchString))
                        {
                            desc1 = desc1.Replace(searchString, replaceString);
                        }

                        break;
                    }
                }

                terminal.Description1 = desc1;
            }
        }

        
    }
}