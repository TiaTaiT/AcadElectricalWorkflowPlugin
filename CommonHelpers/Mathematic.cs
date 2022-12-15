using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers
{
    public static class Mathematic
    {
        public static bool AreRangesIntersect((int, int) interval1, (int, int) interval2)
        {
            int start1;
            int start2;
            int end1;
            int end2;
            if (interval1.Item1 < interval1.Item2)
            {
                start1 = interval1.Item1;
                end1 = interval1.Item2;
            }
            else
            {
                start1 = interval1.Item2;
                end1 = interval1.Item1;
            }
            if (interval2.Item1 < interval2.Item2)
            {
                start2 = interval2.Item1;
                end2 = interval2.Item2;
            }
            else
            {
                start2 = interval2.Item2;
                end2 = interval2.Item1;
            }

            return (start1 <= end2 && end1 >= start2);
        }
    }
}
