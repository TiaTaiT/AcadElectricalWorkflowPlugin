using AutocadCommands.Models;
using CommonHelpers;
using LinkCommands.Services;
using System.Collections.Generic;
using System.Linq;

using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;

namespace AutocadCommands.Services
{
    public class WiresLinker : CommandPrototype
    {
        private List<(ObjectId, ObjectId)> _sourceDestPairIds = new();
        private ComponentsFactory _componentsFactory;

        public WiresLinker(Document doc) : base(doc)
        {
        }

        public override bool Init()
        {
            CreateComponentsFactory();

            var escape = false;

            while (!escape)
            {
                var options1 = new PromptEntityOptions("\nSelect source wire: ");
                options1.SetRejectMessage("not valid Object \n");
                options1.AddAllowedClass(typeof(Line), true);
                options1.AddAllowedClass(typeof(Polyline), true);

                //Make the selection   
                var selectionBlock1 = _ed.GetEntity(options1);//_ed.GetSelection(opts, filter);
                if (selectionBlock1.Status != PromptStatus.OK)
                {
                    if (_sourceDestPairIds.Count == 0)
                        return false;
                    else return true;
                }

                var sourceWireId = selectionBlock1.ObjectId;

                HighlightObject(sourceWireId);

                var options2 = new PromptEntityOptions("\nSelect destination wire: ");
                options2.SetRejectMessage("not valid Object \n");
                options2.AddAllowedClass(typeof(Line), true);
                options2.AddAllowedClass(typeof(Polyline), true);
                //Make the selection   
                var selectionBlock2 = _ed.GetEntity(options2);//_ed.GetSelection(opts, filter);
                if (selectionBlock2.Status != PromptStatus.OK)
                {
                    if (_sourceDestPairIds.Count == 0)
                        return false;
                    else return true;
                }

                var destinationWireId = selectionBlock2.ObjectId;

                if (sourceWireId == destinationWireId)
                {
                    _ed.WriteMessage("Source and destination wire are the same!");
                    return false;
                }

                HighlightObject(destinationWireId);

                _sourceDestPairIds.Add((sourceWireId, destinationWireId));
            }
            return true;
        }

        private void CreateComponentsFactory()
        {
            using var tr = _db.TransactionManager.StartTransaction();
            _componentsFactory = new ComponentsFactory(_db, _tr);
            tr.Dispose();
        }

        private void HighlightObject(ObjectId sourceWireId)
        {
            using var tr = _db.TransactionManager.StartTransaction();
            Entity ent = (Entity)tr.GetObject(sourceWireId, OpenMode.ForWrite);
            ent.Highlight();
        }

        private void UnhighlightObject(ObjectId sourceWireId)
        {
            using var tr = _db.TransactionManager.StartTransaction();
            Entity ent = (Entity)tr.GetObject(sourceWireId, OpenMode.ForWrite);
            ent.Unhighlight();
        }

        public override void Run()
        {
            _doc.LockDocument();
            using var tr = _db.TransactionManager.StartTransaction();
            for (var i = 0; i < _sourceDestPairIds.Count(); i++)
            {
                var wire = new Wire(_tr, 
                                    _sourceDestPairIds[i].Item1,
                                    _sourceDestPairIds[i].Item2,
                                    _componentsFactory.Components);
                wire.Create();
            }

            UnhighlightAllPassedObjects();
            tr.Commit();
        }

        private void UnhighlightAllPassedObjects()
        {
            foreach (var pair in _sourceDestPairIds)
            {
                UnhighlightObject(pair.Item1);
                UnhighlightObject(pair.Item2);
            }
        }
    }
}
