using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutocadCommands.Models;

namespace AutocadCommands.Utils
{
    public class TerminalsComparer : IComparer<Terminal>
    {
        private const double ToleranceBetweenLines = 0.5;

        public int Compare(Terminal a, Terminal b)
        {
            if (a != null && b != null)
            {
                var x1 = a.X;
                var y1 = a.Y;
                var x2 = b.X;
                var y2 = b.Y;

                if (a.Name.StartsWith("V") && b.Name.StartsWith("V"))
                {
                    if (Math.Abs(y1 - y2) < ToleranceBetweenLines)
                    {
                        if (x1 < x2) return -1;
                        if (x1 > x2) return 1;
                    }

                    if (y1 < y2) return 1;
                    if (y1 > y2) return -1;
                }
                if (a.Name.StartsWith("H") && b.Name.StartsWith("H"))
                {
                    if (Math.Abs(x1 - x2) < ToleranceBetweenLines)
                    {
                        if (y1 < y2) return -1;
                        if (y1 > y2) return 1;
                    }

                    if (x1 < x2) return 1;
                    if (x1 > x2) return -1;
                }
            }

            return 0;
        }
    }
}
