using Autodesk.AutoCAD.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
