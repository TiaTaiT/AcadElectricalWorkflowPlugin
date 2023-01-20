using AutocadCommands.Models;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using CommonHelpers;
using LinkCommands.Models;
using LinkCommands.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AutocadCommands.Services
{
    public class MultiWiresLinker : CommandPrototype
    {
        private List<Entity> _multiWireEntities = new();
        private List<MultiWire> _multiWires = new();
        private ComponentsFactory _componentsFactory;
        private NetsFactory _netsFactory;

        public MultiWiresLinker(Document doc) : base(doc)
        {
        }

        public override bool Init()
        {
            CreateComponentsFactory();

            //Make the selection   
            var selectedItems = _ed.SelectImplied();

            if (selectedItems.Status == PromptStatus.OK)
            {
                var objectIds = selectedItems.Value.GetObjectIds();

                // Clear the PickFirst selection set
                var idarrayEmpty = new ObjectId[0];
                _ed.SetImpliedSelection(idarrayEmpty);

                _multiWireEntities = GetMultiWireEntities(objectIds).ToList();

                if (_multiWireEntities.Count != 0)
                    return true;
            }

            var promptResult = _ed.GetString("\nSelect polyline multiwires [All Select] <Select>: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;

            var selectMethod = promptResult.StringResult?.ToUpper();
            if (selectMethod == null)
                return false;

            if (selectMethod == "A" || selectMethod == "А")
            {
                _multiWireEntities = GetMultiWireEntities(LinkerHelper.SelectAllPolylineByLayer(_ed, Layers.MultiWires)).ToList();
            }

            if (selectMethod == "S" || selectMethod == "Ы" || selectMethod == "")
            {
                var filter = new SelectionFilter(
                    new TypedValue[] {
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "<and"),
                    new TypedValue(Convert.ToInt32(DxfCode.LayerName), Layers.MultiWires),
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "<or"),
                    new TypedValue(Convert.ToInt32(DxfCode.Start), "LWPOLYLINE"),
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "or>"),
                    new TypedValue(Convert.ToInt32(DxfCode.Operator), "and>")
                });

                var opts = new PromptSelectionOptions
                {
                    MessageForAdding = "Select polyline multiwires: "
                };

                //Make the selection   

                var selectionBlocks = _ed.GetSelection(opts, filter);
                if (selectionBlocks.Status != PromptStatus.OK)
                    return false;

                _multiWireEntities = GetMultiWireEntities(selectionBlocks.Value.GetObjectIds()).ToList();
            }

            return _multiWireEntities.Any();
        }

        private void CreateComponentsFactory()
        {
            _componentsFactory = new ComponentsFactory(_db);
            _netsFactory = new NetsFactory(_db, _componentsFactory.GetTerminalPoints());
            var terminals = _componentsFactory.GetAllTerminalsInComponents();
            ComponentsWiresTier.CreateElectricalNet(terminals, _netsFactory.Wires);
            DebugTier();

        }

        private void DebugTier()
        {
            foreach (var component in _componentsFactory.Components)
            {
                Debug.WriteLine(component.Name.ToString() + "; terminals: " + component.Terminals.Count);
                foreach (var terminal in component.Terminals)
                {
                    Debug.WriteLine("    - " + terminal.Value + "; Wires = " + terminal.ConnectedWires.Count);
                }
            }
        }

        private IEnumerable<Entity> GetMultiWireEntities(ObjectId[] objectIds)
        {
            using var acTrans = _db.TransactionManager.StartTransaction();
            foreach (ObjectId id in objectIds)
            {
                var entity = (Entity)acTrans.GetObject(id, OpenMode.ForRead);
                if (IsMultiWire(entity))
                    yield return entity;
            }
            _ed.WriteMessage("Number of objects in Pickfirst selection: " + _multiWireEntities.Count());
        }

        private static bool IsMultiWire(Entity entity)
        {
            //if(id.ObjectClass.DxfName == "LWPOLYLINE") it's the fastest!
            var entityType = entity.GetType();

            return entityType == typeof(Polyline) && entity.Layer.StartsWith(Layers.MultiWires);
        }

        public override void Run()
        {
            _doc.LockDocument();
            using var tr = _db.TransactionManager.StartTransaction();
            foreach (Entity entity in _multiWireEntities)
            {
                var polyLine = (Polyline)entity;
                var multiWire = new MultiWire(polyLine, _componentsFactory.Components);
                multiWire.Clean();
                multiWire.Create();
            }
            tr.Commit();
        }
    }
}
