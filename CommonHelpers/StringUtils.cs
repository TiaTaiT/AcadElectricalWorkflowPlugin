using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            var result = new List<string>();
            var numbStr = new StringBuilder();
            var lastChar = '\0';
            var isChangeStateRequired = false;

            for (var i = 0; i < str.Length; i++)
            {
                if ((result.Count() > 0 || numbStr.Length >= 1) &&
                    (str[i] == 'i' && lastChar != str[i]))
                {
                    result.Add(numbStr.ToString());
                    numbStr.Clear();

                    result.Add(str[i].ToString());

                    lastChar = str[i];
                    continue;
                }
                if (IsCharDigitWithPoint(lastChar) ^ IsCharDigitWithPoint(str[i]) && lastChar != 'i' && (numbStr.Length > 0))
                {
                    result.Add(numbStr.ToString());
                    numbStr.Clear();
                }
                numbStr.Append(str[i]);
                lastChar = str[i];
            }
            result.Add(numbStr.ToString());

            return result;
        }

        public static string RemoveCharacters(this string s, params char[] unwantedCharacters)
        => s == null ? null : string.Join(string.Empty, s.Split(unwantedCharacters));

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
            if (c == '.')
                return true;
            if (char.IsDigit(c))
                return true;
            return false;
        }

        public static string RemovePrefix(string str)
        {
            var result = new StringBuilder();
            var isFirst = true;
            foreach (char c in str)
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
