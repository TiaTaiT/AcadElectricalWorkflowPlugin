using CommonHelpers.Models;
using System;
using System.Collections.Generic;

namespace LinkCommands.Services
{
    public static class LinkSymbolNameResolver
    {
        private const char _up = '4';
        private const char _down = '2';
        private const char _right = '3';
        private const char _left = '1';

        private const string _symbolPrefix = "HA";
        
        private const string _sourceSymbolCode = "S";
        private const string _destinationSymbolCode = "D";


        public const string _symbolTypeWave = "4";
        public const string _symbolTypeHexagon = "3";

        public static string GetDestinationName(IAutocadDirectionEnum.Direction destinationDirection, string symbolTypeWave)
        {
            return destinationDirection switch
            {
                IAutocadDirectionEnum.Direction.Right => _symbolPrefix + symbolTypeWave + _destinationSymbolCode + _right,
                IAutocadDirectionEnum.Direction.Above => _symbolPrefix + symbolTypeWave + _destinationSymbolCode + _up,
                IAutocadDirectionEnum.Direction.Left => _symbolPrefix + symbolTypeWave + _destinationSymbolCode + _left,
                IAutocadDirectionEnum.Direction.Below => _symbolPrefix + symbolTypeWave + _destinationSymbolCode + _down,
                _ => _symbolPrefix + symbolTypeWave + _destinationSymbolCode + _up,
            };
        }

        public static string GetSourceName(IAutocadDirectionEnum.Direction sourceDirection, string symbolTypeWave)
        {
            return sourceDirection switch
            {
                IAutocadDirectionEnum.Direction.Right => _symbolPrefix + symbolTypeWave + _sourceSymbolCode + _right,
                IAutocadDirectionEnum.Direction.Above => _symbolPrefix + symbolTypeWave + _sourceSymbolCode + _up,
                IAutocadDirectionEnum.Direction.Left => _symbolPrefix + symbolTypeWave + _sourceSymbolCode + _left,
                IAutocadDirectionEnum.Direction.Below => _symbolPrefix + symbolTypeWave + _sourceSymbolCode + _down,
                _ => _symbolPrefix + symbolTypeWave + _sourceSymbolCode + _down,
            };
        }

        public static IEnumerable<string> GetSourceSymbolNames()
        {
            var sourceNames = new List<string>
            {
                _symbolPrefix + _symbolTypeWave + _sourceSymbolCode + _up,
                _symbolPrefix + _symbolTypeWave + _sourceSymbolCode + _left,
                _symbolPrefix + _symbolTypeWave + _sourceSymbolCode + _down,
                _symbolPrefix + _symbolTypeWave + _sourceSymbolCode + _right,

                _symbolPrefix + _symbolTypeHexagon + _sourceSymbolCode + _up,
                _symbolPrefix + _symbolTypeHexagon + _sourceSymbolCode + _left,
                _symbolPrefix + _symbolTypeHexagon + _sourceSymbolCode + _down,
                _symbolPrefix + _symbolTypeHexagon + _sourceSymbolCode + _right
            };
            return sourceNames;
        }

        public static IEnumerable<string> GetDestinationSymbolNames()
        {
            var destinationNames = new List<string>
            {
                _symbolPrefix + _symbolTypeWave + _destinationSymbolCode + _up,
                _symbolPrefix + _symbolTypeWave + _destinationSymbolCode + _left,
                _symbolPrefix + _symbolTypeWave + _destinationSymbolCode + _down,
                _symbolPrefix + _symbolTypeWave + _destinationSymbolCode + _right,

                _symbolPrefix + _symbolTypeHexagon + _destinationSymbolCode + _up,
                _symbolPrefix + _symbolTypeHexagon + _destinationSymbolCode + _left,
                _symbolPrefix + _symbolTypeHexagon + _destinationSymbolCode + _down,
                _symbolPrefix + _symbolTypeHexagon + _destinationSymbolCode + _right,
            };
            return destinationNames;
        }

        /// <summary>
        /// Get list of all possible names of link symbols
        /// </summary>
        public static IEnumerable<string> GetAllNames()
        {
            var directions = (IAutocadDirectionEnum.Direction[])Enum.GetValues(typeof(IAutocadDirectionEnum.Direction));
            foreach (var direction in directions)
            {
                yield return GetSourceName(direction, _symbolTypeHexagon);
            }

            foreach (var direction in directions)
            {
                yield return GetDestinationName(direction, _symbolTypeHexagon);
            }

            foreach (var direction in directions)
            {
                yield return GetSourceName(direction, _symbolTypeWave);
            }

            foreach (var direction in directions)
            {
                yield return GetDestinationName(direction, _symbolTypeWave);
            }
        }
    }
}
