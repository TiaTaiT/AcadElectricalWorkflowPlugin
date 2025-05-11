using Teigha.Geometry;

namespace AutocadTerminalsManager.Model
{
    public class AttrInfo
    {
        public AttrInfo(Point3d pos, Point3d aln, bool aligned)
        {
            Position = pos;
            Alignment = aln;
            IsAligned = aligned;
        }

        public Point3d Position { set; get; }

        public Point3d Alignment { set; get; }

        public bool IsAligned { set; get; }

        public string Layer { set; get; }
    }
}