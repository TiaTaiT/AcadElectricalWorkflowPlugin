using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Internal;
using System.Collections.Generic;
using System.Linq;
using AutocadCommands.Models;
using AutocadCommands.Utils;
using CommonHelpers;
using static System.Int32;

namespace AutocadCommands.Services
{
    public class TermRenumber : CommandPrototype
    {
        private int startNumb;
        private PromptSelectionResult selectionBlocks;

        public TermRenumber(Document doc) : base(doc)
        {
        }

        public override bool Init()
        {
            var promptResult = _ed.GetString("\nEnter the start number: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;
            if (!TryParse(promptResult.StringResult, out startNumb))
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
            selectionBlocks = _ed.GetSelection(opts, filter);
            return selectionBlocks.Status == PromptStatus.OK;
        }

        public override void Run()
        {
            /*
            var promptResult = _ed.GetString("\nEnter the start number: ");
            if (promptResult.Status != PromptStatus.OK)
                return;
            if (!TryParse(promptResult.StringResult, out var startNumb))
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
            var res = _ed.GetSelection(opts, filter);
            if (res.Status != PromptStatus.OK)
                return;
            */


            var terminals = new List<Terminal>();

            // Lock the document
            using var acLckDoc = _doc.LockDocument();
            var objIds = new ObjectIdCollection(selectionBlocks.Value.GetObjectIds());

            using (var acTrans = _db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId blkId in objIds)
                {
                    /*var newTerminal = GetTerminal(acTrans, blkId);
                        terminals.Add(newTerminal);
                        */
                    terminals = TerminalsHelper.GetTerminals(acTrans, objIds, false);
                }

                acTrans.Commit();
            }

            using (var acTrans = _db.TransactionManager.StartTransaction())
            {
                IComparer<Terminal> comparer = new TerminalsComparer();
                terminals.Sort(comparer);
                AutoNumb(terminals, startNumb);
                TerminalsHelper.SetTerminals(acTrans, objIds, terminals);
                acTrans.Commit();
            }
        }

        private void AutoNumb(IEnumerable<Terminal> terminals, int startNumb)
        {
            var counter = startNumb;
            foreach (var terminal in terminals)
            {
                terminal.TerminalNumber = counter;
                counter++;
            }
        }
    }
}