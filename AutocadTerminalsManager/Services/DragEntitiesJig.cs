using System.Collections.Generic;

namespace AutocadTerminalsManager.Services
{
    public class DragEntitiesJig : DrawJig
    {
        private Point3d _basePoint, _dragPoint;
        private readonly IEnumerable<Entity> _entities;
        private Matrix3d _displacement;

        public DragEntitiesJig(IEnumerable<Entity> entities, Point3d basePoint)
        {
            _basePoint = basePoint;
            _entities = entities;
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            var wGeom = draw.Geometry;
            _displacement = Matrix3d.Displacement(_basePoint.GetVectorTo(_dragPoint));
            wGeom.PushModelTransform(_displacement);
            foreach (var entity in _entities)
            {
                wGeom.Draw(entity);

            }
            wGeom.PopModelTransform();
            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var options = new JigPromptPointOptions("\nSecond point: ")
            {
                BasePoint = _basePoint,
                UseBasePoint = true,
                Cursor = CursorType.RubberBand,
                UserInputControls = UserInputControls.Accept3dCoordinates
            };
            var ppr = prompts.AcquirePoint(options);
            if (ppr.Value.IsEqualTo(_dragPoint))
                return SamplerStatus.NoChange;
            _dragPoint = ppr.Value;

            return SamplerStatus.OK;
        }

        public void TransformEntities()
        {
            foreach (var entity in _entities)
            {
                if (entity == null) continue;
                entity.UpgradeOpen();
                entity.TransformBy(_displacement);
                entity.DowngradeOpen();
            }

        }
    }
}