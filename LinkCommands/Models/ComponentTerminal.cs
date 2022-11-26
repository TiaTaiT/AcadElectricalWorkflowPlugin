using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace LinkCommands.Models
{
    public class ComponentTerminal
    {
        /// <summary>
        /// Zero coordinates
        /// </summary>
        public IEnumerable<Point3d> Points { get; set; }

        public string Tag { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }

        public ComponentTerminal()
        {
        }

        public ComponentTerminal(IEnumerable<Point3d> points, string tag, string value)
        {
            Tag = tag;
            Value = value;
            Points = points;
        }

        /// <summary>
        /// List of electrically connected terminals
        /// </summary>
        public List<ComponentTerminal> TiedTerminals { get; set; } = new();

        /// <summary>
        /// Check if point3d is one of the terminal connection points
        /// </summary>
        /// <param name="point3d">point for checking</param>
        /// <returns>true if point3d contains in terminal connction point list</returns>
        public bool IsContainPoint(Point3d point3d)
        {
            foreach(var point in Points)
            {
                if (point.Equals(point3d))
                    return true;
            }
            return false;
        }
    }
}