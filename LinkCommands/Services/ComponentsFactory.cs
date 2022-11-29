using AutocadCommands.Helpers;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.MacroRecorder;
using CommonHelpers;
using LinkCommands.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LinkCommands.Services
{
    public class ComponentsFactory
    {
        private int _index = 0;
        private Database _db;
        private const string ComponentSign = "CAT";
        private const string Description1 = "DESC1";
        private const string TerminalDescriptionSign = "TERM";
        private const int MaxDegreesNumber = 4; // If this value is more than 4 AutoCAD may generate exception "eAtMaxReaders"
                                                // The exception comes since the object is opened for read more than 255 times.
                                                // It depends on object amount

        public IEnumerable<ElectricalComponent> Components { get; set; } = Enumerable.Empty<ElectricalComponent>();

        public ComponentsFactory(Database db)
        {
            _db = db;

            using var tr = _db.TransactionManager.StartTransaction();
            FindElectricalComponents();
            tr.Dispose();
        }

        private void FindElectricalComponents()
        {
            var componentsList = new ConcurrentBag<ElectricalComponent>();

            var blkRefs = AttributeHelper.GetObjectsStartWith(_db, ComponentSign);

            Parallel.ForEach(blkRefs, new ParallelOptions() { MaxDegreeOfParallelism = MaxDegreesNumber },
                (item, i) => CreateElectricalComponent(item, componentsList));

            Components = componentsList;
        }

        private void CreateElectricalComponent(BlockReference blkRef, ConcurrentBag<ElectricalComponent> componentsList)
        {
            if (string.IsNullOrEmpty(blkRef?.Name))
                return;

            var component = GetComponent(blkRef);

            component.BlockRef = blkRef;
            componentsList.Add(component);
        }

        private ElectricalComponent GetComponent(BlockReference blkRef)
        {
            var attributes = blkRef.AttributeCollection;
            var attrDict = AttributeHelper.GetAttributesFromCollection(attributes);

            var designation = GetTagValue(attrDict);
            var name = GetNameValue(attrDict);

            if (designation == null || name == null)
                throw new Exception("designation or/and name of component is null");

            var terminals = GetTerminals(attributes, attrDict).ToList();

            return new ElectricalComponent(_index++, name, designation, terminals, blkRef);
        }

        private string GetNameValue(Dictionary<string, string> attrDict)
        {
            if(attrDict.TryGetValue(Description1, out var strValue))
                return strValue;
            return string.Empty;
        }

        private string GetTagValue(Dictionary<string, string> attrDict)
        {
            foreach (var item in attrDict)
            {
                if (item.Key.ToUpper().Equals(ComponentSign))
                {
                    return item.Value;
                }
            }
            return string.Empty;
        }

        private IEnumerable<ComponentTerminal> GetTerminals(AttributeCollection attributes, Dictionary<string,string> attrDict)
        {
            var terminals = new List<ComponentTerminal>();
            foreach(var attr in attrDict)
            {
                if(attr.Key.ToUpper().StartsWith(TerminalDescriptionSign))
                {
                    var connectionNames = GetConnectionAttributeName(attr.Key, attrDict);
                    var connectionPoints = GetConnectionPoints(attributes, connectionNames);
                    terminals.Add(new ComponentTerminal(connectionPoints, attr.Key, attr.Value));
                }
            }
            return terminals;
        }

        private IEnumerable<Point3d> GetConnectionPoints(AttributeCollection attributes, IEnumerable<string> names)
        {
            var points = new List<Point3d>();
            foreach (ObjectId attId in attributes)
            {

                AttributeReference att;
                
                try
                {
                    att = (AttributeReference)attId.GetObject(OpenMode.ForRead, false);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Error!!!" + ex.Message);
                    continue;
                }
                
                if (att == null)
                    continue;

                foreach(var name in names)
                {
                    if (att.Tag.Equals(name))
                        points.Add(att.Position);
                }
            }
            return points;
        }

        private IEnumerable<string> GetConnectionAttributeName(string termTag, Dictionary<string, string> attrDict)
        {
            foreach(var attr in attrDict)
            {
                if(attr.Key.ToUpper().StartsWith("X") && attr.Key.ToUpper().EndsWith(termTag))
                {
                    yield return attr.Key;
                }
            }
            
        }
    }
}
