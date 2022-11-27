using AutocadCommands.Helpers;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.MacroRecorder;
using CommonHelpers;
using LinkCommands.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LinkCommands.Services
{
    public class ComponentsFactory
    {
        private readonly Database _db;
        private int _index = 0;

        private const string ComponentSign = "CAT";
        private const string Description1 = "DESC1";
        private const string TerminalDescriptionSign = "TERM";

        public IEnumerable<ElectricalComponent> Components { get; set; } = Enumerable.Empty<ElectricalComponent>();

        public ComponentsFactory(Database db)
        {
            _db = db;
            FindElectricalComponents();
        }

        private void FindElectricalComponents()
        {
            var componentsList = new List<ElectricalComponent>();

            var blkRefs = AttributeHelper.GetObjectsStartWith(_db, ComponentSign);
            foreach (var blkRef in blkRefs)
            {
                componentsList.Add(CreateElectricalComponent(blkRef));
            }
            Components = componentsList;
        }

        private ElectricalComponent CreateElectricalComponent(BlockReference blkRef)
        {
            var component = GetComponent(blkRef);

            component.BlockRef = blkRef;
            return component;
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
            foreach(var attr in attrDict)
            {
                if(attr.Key.ToUpper().StartsWith(TerminalDescriptionSign))
                {
                    var connectionNames = GetConnectionAttributeName(attr.Key, attrDict);
                    var connectionPoints = GetConnectionPoints(attributes, connectionNames);
                    yield return new ComponentTerminal(connectionPoints, attr.Key, attr.Value);
                }
            }
        }

        private IEnumerable<Point3d> GetConnectionPoints(AttributeCollection attributes, IEnumerable<string> names)
        {
            var points = new List<Point3d>();
            foreach (ObjectId attId in attributes)
            {
                var att = (AttributeReference)attId.GetObject(OpenMode.ForRead, false);
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
