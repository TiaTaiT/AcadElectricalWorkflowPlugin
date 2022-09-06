using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers
{
    public static class Mathematic
    {
        public static bool IsNumeric(string s)
        {
            foreach (char c in s)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        public static List<int> GetAllNumbers(string str)
        {
            var nums = new List<int>();
            var start = -1;
            for (int i = 0; i < str.Length; i++)
            {
                if (start < 0 && Char.IsDigit(str[i]))
                {
                    start = i;
                }
                else if (start >= 0 && !Char.IsDigit(str[i]))
                {
                    nums.Add(int.Parse(str.Substring(start, i - start)));
                    start = -1;
                }
            }
            if (start >= 0)
                nums.Add(int.Parse(str.Substring(start, str.Length - start)));
            return nums;
        }

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
