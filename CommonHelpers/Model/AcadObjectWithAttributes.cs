using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace AutocadTerminalsManager.Model
{
    public class AcadObjectWithAttributes
    {
        public Entity Entity { get; set; }
        public Dictionary<string, string> Attributes { get; set; }

        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }

        public ObjectId GetId { get => Entity.ObjectId; }
    }
}