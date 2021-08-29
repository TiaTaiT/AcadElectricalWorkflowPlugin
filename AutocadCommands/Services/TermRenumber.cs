using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using AutocadCommands.Models;
using AutocadCommands.Utils;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutocadCommands.Services
{
    public class TermRenumber
    {
        private Editor _ed;
        private Document _doc;
        private Database _db;

        public TermRenumber(Editor ed, Document doc, Database db)
        {
            _ed = ed;
            _doc = doc;
            _db = db;
        }

        public void Run()
        {
            var promptResult = _ed.GetString("\nEnter the start number: ");
            if (promptResult.Status != PromptStatus.OK)
                return;
            if (!Int32.TryParse(promptResult.StringResult, out var startNumb))
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



            var terminals = new List<Terminal>();

            // Lock the document
            using (DocumentLock acLckDoc = _doc.LockDocument())
            {
                var objIds = new ObjectIdCollection(res.Value.GetObjectIds());

                using (Transaction acTrans = _db.TransactionManager.StartTransaction())
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

                using (Transaction acTrans = _db.TransactionManager.StartTransaction())
                {
                    IComparer<Terminal> comparer = new TerminalsComparer();
                    terminals.Sort(comparer);
                    AutoNumb(terminals, startNumb);
                    TerminalsHelper.SetTerminals(acTrans, objIds, terminals);
                    acTrans.Commit();
                }
            }
        }

        private void AutoNumb(List<Terminal> terminals, int startNumb)
        {
            int counter = startNumb;
            foreach (var terminal in terminals)
            {
                terminal.TerminalNumber = counter;
                counter++;
            }
        }

    }
}