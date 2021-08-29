using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace AutocadTerminalsManager.Model
{
    public class AcadObjectWithAttributes
    {
        public Entity Entity { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
    }
}