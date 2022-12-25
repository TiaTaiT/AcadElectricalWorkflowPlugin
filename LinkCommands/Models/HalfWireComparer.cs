using LinkCommands.Models;
using System;
using System.Collections.Generic;

namespace AutocadCommands.Models
{
    public class HalfWireComparer : IComparer<HalfWire>
    {
        private const double _tolerance = 0.1;
        public int Compare(HalfWire first, HalfWire second)
        {
            var x1 = first.PointConnectedToMultiWire.X;
            var y1 = first.PointConnectedToMultiWire.Y;
            var x2 = second.PointConnectedToMultiWire.X;
            var y2 = second.PointConnectedToMultiWire.Y;

            if (Math.Abs(y2 - y1) < _tolerance)
            {
                if (x2 - x1 > _tolerance)
                    return -1;

                if (x2 - x1 < 0 - _tolerance)
                    return 1;

                return 0;
            }
            if (Math.Abs(y2 - y1) > _tolerance)
            {
                if (y2 - y1 > _tolerance)
                    return -1;
                if (y2 - y1 < 0 - _tolerance)
                    return 1;
                return 0;
            }
            return 0;
        }
    }
}