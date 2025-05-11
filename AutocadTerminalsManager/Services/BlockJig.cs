using AutocadTerminalsManager.Model;
using System.Collections.Generic;

using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
using Teigha.Geometry;

namespace AutocadTerminalsManager.Services
{
    internal class BlockJig : EntityJig
    {
        private Point3d _pos;
        //private string _layer;
        private Dictionary<ObjectId, AttrInfo> _attInfo;
        private Transaction _tr;

        public BlockJig(Transaction tr, BlockReference br, Dictionary<ObjectId, AttrInfo> attInfo) : base(br)
        {
            _pos = br.Position;
            //_layer = br.Layer;
            _attInfo = attInfo;
            _tr = tr;
        }

        protected override bool Update()
        {
            var br = (BlockReference)Entity;
            br.Position = _pos;
            //br.Layer = _layer;

            if (br.AttributeCollection.Count == 0) return true;
            foreach (ObjectId id in br.AttributeCollection)
            {
                var attrRef = (AttributeReference)_tr.GetObject(id, OpenMode.ForRead);
                // Apply block transform to att def position
                if (attrRef == null) continue;

                attrRef.UpgradeOpen();
                var attrInfo = _attInfo[attrRef.ObjectId];
                attrRef.Position = attrInfo.Position.TransformBy(br.BlockTransform);
                if (attrInfo.IsAligned)
                {
                    attrRef.AlignmentPoint = attrInfo.Alignment.TransformBy(br.BlockTransform);
                }
                if (attrRef.IsMTextAttribute)
                {
                    attrRef.UpdateMTextAttribute();
                }

                attrRef.Layer = attrInfo.Layer;
                attrRef.DowngradeOpen();
            }
            return true;
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var opts = new JigPromptPointOptions("\nSelect insertion point:")
            {
                BasePoint = new Point3d(0, 0, 0),
                UserInputControls = UserInputControls.NoZeroResponseAccepted
            };
            var ppr = prompts.AcquirePoint(opts);
            if (_pos == ppr.Value)
            {
                return SamplerStatus.NoChange;
            }
            _pos = ppr.Value;
            return SamplerStatus.OK;
        }

        public PromptStatus Run()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var promptResult = ed.Drag(this);
            return promptResult.Status;
        }
    }
}