using AutocadCommands.Model;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutocadCommands.Models
{
    public class MultiWire
    {
        private double tolerance = 1;

        public Entity MultiWireEntity { get; set; }

        public List<Wire> ConnectedWires { get; set; }

        public IEnumerable<(Wire sourceWire, Wire destinationWire)> GetSourceDestinationWirePairs()
        {
            ConnectedWires.Sort(new OrderingPairWires());

            var index = GetSourcesIndex();

            var sourceWires = ConnectedWires.GetRange(0, index);
            var destWires = ConnectedWires.GetRange(index, (ConnectedWires.Count) - index);

            var maxCount = 0;

            if (sourceWires.Count > destWires.Count)
            {
                maxCount = destWires.Count;
            }
            else
            {
                maxCount = sourceWires.Count;
            }

            for (var i = 0; i < maxCount; i++)
            {
                yield return (sourceWires[i], destWires[i]);
            }
        }

        private int GetSourcesIndex()
        {
            // Two horizontal lines
            for(var i = 0; i < ConnectedWires.Count - 1; i++)
            {
                var y1 = ConnectedWires[i].PointConnectedToMultiWire.Y;
                var y2 = ConnectedWires[i+1].PointConnectedToMultiWire.Y;
                if ((y2 - y1) > tolerance)
                    return i + 1;
            }
            // Two vertical lines
            for (var i = 0; i < ConnectedWires.Count - 1; i++)
            {
                var x1 = ConnectedWires[i].PointConnectedToMultiWire.X;
                var x2 = ConnectedWires[i + 1].PointConnectedToMultiWire.X;
                if ((x2 - x1) > tolerance)
                    return i + 1;
            }
            // If wires connected in one line
            if(ConnectedWires.Count % 2 == 0)
            {
                return ConnectedWires.Count / 2 - 1;
            }
            // If odd wires
            return (ConnectedWires.Count - 1) / 2;
        }
    }
}
