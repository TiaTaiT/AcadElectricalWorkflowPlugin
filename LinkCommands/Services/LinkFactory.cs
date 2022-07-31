using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutocadCommands.Services
{
    public class LinkFactory
    {
        /*

        private PromptSelectionResult _selectionBlocks;

        public IEnumerable<FullWiresBus> GetFullWiresBus(Database db, IEnumerable<Polyline> polylines)
        {
            foreach (var multiWire in polylines)
            {
                var connectedWires = FindAllConnectedWires(db, multiWire);
                if (connectedWires.Any())
                {
                    yield return new FullWiresBus()
                    {
                        MultiWireEntity = multiWire,
                        ConnectedWires = connectedWires.ToList()
                    });

                }


            }
        }

        private IEnumerable<Wire> FindAllConnectedWires(Database db, Entity multiWireEntity)
        {
            var allWires = GetAllWiresFromSheet(db, Layers.MultiWires);
            foreach (var wireEntity in allWires)
            {
                var wire = LinkerHelper.GetWireConnectedToMultiWire(db, multiWireEntity, wireEntity);
                if (wire != null)
                {
                    yield return wire;
                }
            }
        }

        private IEnumerable<Entity> GetAllWiresFromSheet(Database db, string layer)
        {
            // Get all lines and lwpolylines
            var wires = GetIdsUtils.GetIdsByType<Line>(db, layer).ToList();
            wires.AddRange(GetIdsUtils.GetIdsByType<Polyline>(db, layer).ToList());

            //using var acTrans = db.TransactionManager.StartTransaction();
            foreach (ObjectId wireId in wires)
            {
                yield return (Entity)wireId.GetObject(OpenMode.ForRead, false);
            }
        }

        */

    }
}
