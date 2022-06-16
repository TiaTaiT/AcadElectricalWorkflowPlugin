using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutocadCommands.Models.IAutocadDirectionEnum;

namespace AutocadCommands.Models
{
    public class Wire
    {
        public Entity WireEntity { get; set; }

        public Direction Direction { get; set; }

        public Point3d PointConnectedToMultiWire { get; set; }

        // Source/Destination Wire Signal Symbol
        public Entity SignalSymbol { get; set; }

        public Point3d TextCoordinate 
        { 
            get
            {
                var x = PointConnectedToMultiWire.X;
                var y = PointConnectedToMultiWire.Y;
                var space = 1.0;

                return Direction switch
                {
                    Direction.Above => new Point3d(x - space, y + space, 0),
                    Direction.Left => new Point3d(x - space, y + space, 0),
                    Direction.Below => new Point3d(x - space, y - space, 0),
                    Direction.Right => new Point3d(x + space, y + space, 0),
                    _ => new Point3d(x - space, y + space, 0),
                };
            } 
             
        }
        
    }
}
