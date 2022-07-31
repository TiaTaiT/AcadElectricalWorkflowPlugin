using Autodesk.AutoCAD.Geometry;
using LinkCommands.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCommands.Test
{
    internal class CompareWireComparer : IComparer
    {
        private double _tolerance = 0.1;
        public int Compare(object first, object second)
        {
            var x = (CompareWire)first;
            var y = (CompareWire)second;

            if (Math.Abs(x.X - y.X) < _tolerance && Math.Abs(x.Y - y.Y) < _tolerance)
                return 0;
            return -1;
        }
    }
}
