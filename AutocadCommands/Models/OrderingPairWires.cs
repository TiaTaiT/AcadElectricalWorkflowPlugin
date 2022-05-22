using AutocadCommands.Models;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AutocadCommands.Model
{
    public class OrderingPairWires : IComparer<Wire>
    {
        private double _tolerance = 1;

        public int Compare(Wire wire1, Wire wire2)
        {
            if(wire1 == null || wire2 == null)
                return 0;

            var x1 = wire1.PointConnectedToMultiWire.X;
            var y1 = wire1.PointConnectedToMultiWire.Y;
            //var z1 = wire1.PointConnectedToMultiWire.Z;

            var x2 = wire2.PointConnectedToMultiWire.X;
            var y2 = wire2.PointConnectedToMultiWire.Y;
            //var z2 = ((Wire)wire2).PointConnectedToMultiWire.Z;

            // X:
            if(Math.Abs(y1 - y2) < _tolerance)
            {
                if(x1 > x2)
                    return 1;
                if(x1 < x2)
                    return -1;
                return 0;
            }

            if(y1 > y2)
                return 1;
            
            if(y1 < y2)
                return -1;

            return 0;
        }
    }
}
