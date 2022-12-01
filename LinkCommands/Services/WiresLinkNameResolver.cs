using CommonHelpers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkCommands.Services
{
    public static class WiresLinkNameResolver
    {
        private const string _prefix = "ha2";

        private enum DirectionCodes
        {
            Left = '1',
            Down = '2',
            Right = '3',
            Up = '4',
        }

        private const char _source = 's';
        private const char _destination = 'd';

        private const string _suffix = "_inline";

        public static IEnumerable<string> GetAllNames()
        {
            var directions = (DirectionCodes[])Enum.GetValues(typeof(DirectionCodes));
            foreach (var direction in directions)
            {
                yield return _prefix + _source + (char)direction + _suffix;
            }
            foreach (var direction in directions)
            {
                yield return _prefix + _destination + (char)direction + _suffix;
            }
        }
    }
}
