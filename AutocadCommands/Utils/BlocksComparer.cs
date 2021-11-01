using AutocadTerminalsManager.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutocadCommands.Utils
{
    internal class BlocksComparer : IComparer<AcadObjectWithAttributes>
    {
        private const double ToleranceBetweenObjects = 0.5;

        public int Compare(AcadObjectWithAttributes a, AcadObjectWithAttributes b)
        {
            if (a == null || b == null)
                return 0;

            var x1 = a.PositionX;
            var y1 = a.PositionY;
            var x2 = b.PositionX;
            var y2 = b.PositionY;

            if (x1 < x2) return -1;
            if (x1 > x2) return 1;

            return 0;
        }
    }
}
