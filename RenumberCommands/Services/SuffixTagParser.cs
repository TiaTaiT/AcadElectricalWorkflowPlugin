using System;
using RenumberCommands.Interfaces;

namespace RenumberCommands.Services
{
    public class SuffixTagParser : ITagParser
    {
        public (string Prefix, int? Number) Parse(string tagValue)
        {
            if (string.IsNullOrEmpty(tagValue))
                throw new ArgumentException(nameof(tagValue));

            int pos = tagValue.Length;
            while (pos > 0 && char.IsDigit(tagValue[pos - 1])) pos--;
            var prefix = tagValue.Substring(0, pos);
            var numPart = tagValue.Substring(pos);
            int number = 0;
            return (prefix, int.TryParse(numPart, out number) ? number : (int?)null);
        }
    }
}
