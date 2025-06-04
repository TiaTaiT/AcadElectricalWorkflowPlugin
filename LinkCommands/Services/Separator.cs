using LinkCommands.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkCommands.Services
{
    public class Separator
    {
        public List<HalfWire> Sources { get; private set; } = new();
        public List<HalfWire> Destinations { get; private set; } = new();

        /// <summary>
        /// The method divides the original sorted collection into two halves.
        /// First part - sources half wires, second - destinations half wires
        /// </summary>
        /// <param name="sortedHalfWires">Sorted (x or y) half wires collection</param>
        /// <returns>true if succes</returns>
        public bool Separate(IEnumerable<HalfWire> sortedHalfWires)
        {
            if (sortedHalfWires.Count() < 2)
                return false;

            var sourceDestSwitch = false;
            var lastValue = sortedHalfWires.First().PointConnectedToMultiWire.Y;
            Sources.Add(sortedHalfWires.First());

            for (var i = 1; i < sortedHalfWires.Count(); i++)
            {
                var currentWireY = sortedHalfWires.ElementAt(i).PointConnectedToMultiWire.Y;

                if (currentWireY != lastValue)
                {
                    sourceDestSwitch = true;
                }
                if (!sourceDestSwitch)
                {
                    Sources.Add(sortedHalfWires.ElementAt(i));
                }
                else
                {
                    if (Sources.Count() > Destinations.Count())
                    {
                        Destinations.Add(sortedHalfWires.ElementAt(i));
                    }
                }
                lastValue = currentWireY;
            }

            // if multiwire is one line
            if (Sources.Count() > 0 && Destinations.Count() == 0)
            {
                if (Sources.Count() % 2 != 0)
                    return false;

                // split source collection in half
                var halfNumb = Sources.Count() / 2 - 1;
                var max = Sources.Count() - 1;
                for (var j = max; j > halfNumb; j--)
                {
                    Destinations.Add(Sources[j]);
                    Sources.RemoveAt(j);
                }
            }
            return true;
        }
    }
}
