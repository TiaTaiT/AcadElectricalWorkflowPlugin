using AutocadCommands.Models;
using AutocadCommands.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using CommonHelpers;
using LinkCommands.Services;
using System.Collections.Generic;
using static System.Int32;

namespace AutocadCommands.Services
{
    public class TermDescriptionIncrement : CommandPrototype
    {
        private int _startNumber;
        private string _startSequence;
        private PromptSelectionResult _selectedBlocks;

        public TermDescriptionIncrement(Document doc) : base(doc)
        {
        }

        public override bool Init()
        {
            #region Dialog with user

            var promptResult = _ed.GetString("\nEnter the initial character sequence <ШС>: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;

            // If user pass input and press enter
            _startSequence = promptResult.StringResult;
            if (string.IsNullOrEmpty(_startSequence))
                _startSequence = "ШС";

            promptResult = _ed.GetString("\nEnter the start number <1>: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;

            // If user pass input and press enter
            var _startNumberStr = promptResult.StringResult;
            if (string.IsNullOrEmpty(_startNumberStr))
                _startNumberStr = "1";

            if (!TryParse(_startNumberStr, out _startNumber))
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
                IComparer<Terminal> comparer = new TerminalsComparer();
                terminals.Sort(comparer);
                AutoNumb(terminals, _startSequence, _startNumber);
                TerminalsHelper.SetTerminals(acTrans, objIds, terminals);
                acTrans.Commit();
            }
        }

        private void AutoNumb(IEnumerable<Terminal> terminals, string startSequence, int startNumber)
        {
            var isMinus = false;
            var counter = startNumber;
            var parser = new DesignationParser();

            foreach (var terminal in terminals)
            {
                var desc1 = terminal.Description1;
                var designation = parser.GetDesignation(desc1);

                if (!designation.IsShleif) continue;
                
                if (!isMinus)
                {
                    designation.Number = counter.ToString();
                    terminal.Description1 = designation.ToString();
                    isMinus = true; // the next step should be in (isMinus) section
                    continue;
                }

                designation.Number = counter.ToString();
                terminal.Description1 = designation.ToString();
                isMinus = false; // the next step should be in (!isMinus) section
                counter++;                
            }
        }
    }
}