using AutocadCommands.Models;
using AutocadCommands.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using CommonHelpers;
using System.Collections.Generic;

namespace AutocadCommands.Services
{
    public class TermAddDescriptionPrefix : CommandPrototype
    {
        private PromptSelectionResult _selectedBlocks;
        private string _prefix;

        public TermAddDescriptionPrefix(Document doc) : base(doc)
        {
        }

        public override bool Init()
        {
            #region DialogWithUser
            var promptResult = _ed.GetString("\nEnter the prefix character sequence: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;

            _prefix = promptResult.StringResult;
            if (_prefix == null)
                return false;

            var filter = new SelectionFilter(new[]
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
                IComparer<Terminal> comparer = new TerminalsComparer();
                terminals.Sort(comparer);
                AddPrefix(terminals, _prefix);
                TerminalsHelper.SetTerminals(acTrans, objIds, terminals);
                acTrans.Commit();
            }
        }

        private void AddPrefix(IEnumerable<Terminal> terminals, string prefix)
        {
            foreach (var terminal in terminals)
            {
                terminal.Description1 = prefix + terminal.Description1;
            }
        }

    }
}