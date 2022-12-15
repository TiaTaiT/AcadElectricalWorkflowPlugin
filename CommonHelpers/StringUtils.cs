using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers
{
    public static class StringUtils
    {

        public static List<int> GetIntNumbers(string str)
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

        public static IEnumerable<string> GetStringNumbersWithPoint(string str)
        {
            var nums = new List<string>();
            var numbStr = new StringBuilder();

            foreach(var c in str)
            {
                if (IsCharDigitWithPoint(c))
                {
                    numbStr.Append(c);
                    continue;
                }
                yield return numbStr.ToString();
                numbStr.Clear();
            }
        }

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

        public static bool IsCharDigitWithPoint(char c)
        {
            if (c == '+' || c == '-')
                return false;
            if(c == '.')
                return true;
            if(char.IsDigit(c))
                return true;
            return false;
        }

        public static string RemovePrefix(string str)
        {
            var result = new StringBuilder();
            var isFirst = true;
            foreach(char c in str)
            {
                if (IsCharDigitWithPoint(c) && isFirst)
                    continue;
                isFirst = false;
                result.Append(c);
            }
            

            return result.ToString();
        }

        public static string RemoveAfter(this string value, string characters)
        {
            int index = value.IndexOf(characters);
            if (index > 0)
            {
                value = value.Substring(0, index);
            }
            return value;
        }

        public static string RemoveBefore(this string value, string characters)
        {
            int index = value.IndexOf(characters);
            if (index > 0)
            {
                return value.Substring(index);
            }
            return value;
        }

    }
}
