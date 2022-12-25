using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace LinkCommands.Models
{
    public class Link
    {
        public string SigCode { get; set; }
        public string Description { get; set; }
        public BlockReference Reference { get; set; }
        public Point3d WireConnectionPoint { get; set; }
        public Point3d MultiwireConnectionPoint { get; set; }
    }
}
