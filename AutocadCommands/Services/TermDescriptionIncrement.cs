using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutocadCommands.Models;
using AutocadCommands.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using CommonHelpers;
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

            var promptResult = _ed.GetString("\nEnter the initial character sequence: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;

            _startSequence = promptResult.StringResult;
            if (_startSequence == null)
                return false;

            promptResult = _ed.GetString("\nEnter the start number: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;
            if (!TryParse(promptResult.StringResult, out _startNumber))
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

            foreach (var terminal in terminals)
            {
                var desc1 = terminal.Description1;
                if (!desc1.Contains(startSequence)) continue;

                if (startSequence.EndsWith("ШС") ||
                    startSequence.EndsWith("ШСi") ||
                    startSequence.EndsWith("КЦ") ||
                    startSequence.EndsWith("КЦi"))
                {
                    var insertIndex = desc1.LastIndexOf(startSequence, StringComparison.Ordinal) + startSequence.Length;
                    if (insertIndex < 0)
                        return;
                    var cutStr = desc1.Substring(0, insertIndex);
                    if (!isMinus)
                    {
                        terminal.Description1 = cutStr + counter.ToString() + "+";
                        isMinus = true; // the next step should be in (isMinus) section
                    }
                    else
                    {
                        terminal.Description1 = cutStr + counter.ToString() + "-";
                        isMinus = false; // the next step should be in (!isMinus) section
                        counter++;
                    }
                }
                else
                {
                    var insertIndex = desc1.IndexOf(startSequence, StringComparison.Ordinal) + startSequence.Length;
                    var desc1WithoutPrefix = desc1.Substring(insertIndex);
                    var countingNumbStr = GetFirstDigitsNumber(desc1WithoutPrefix);

                    var desc1Suffix = desc1WithoutPrefix.Substring(countingNumbStr.Length);

                    terminal.Description1 = startSequence + counter + desc1Suffix;
                    counter++;
                }
            }
        }

        private string GetFirstDigitsNumber(string desc1WithoutPrefix)
        {
            var numbStr = new StringBuilder();
            foreach (var ch in desc1WithoutPrefix)
            {
                if (char.IsDigit(ch)) numbStr.Append(ch);
                else
                {
                    break;
                }
            }

            return numbStr.ToString();
        }
    }
}