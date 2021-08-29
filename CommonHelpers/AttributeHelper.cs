using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutocadTerminalsManager.Model;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows.BlockStream;

namespace AutocadCommands.Helpers
{
    public static class AttributeHelper
    {
        /// <summary>
        /// Get attribute value.
        /// </summary>
        /// <param name="tr">Autocad database transaction</param>
        /// <param name="attCol">Attribute collection</param>
        /// <param name="tagName">Attribute name</param>
        /// <returns></returns>
        public static string GetAttributeValue(Transaction tr, AttributeCollection attCol, string tagName)
        {
            foreach (ObjectId attId in attCol)
            {
                var att = (AttributeReference)tr.GetObject(attId, OpenMode.ForRead);
                if (att.Tag.Equals(tagName))
                {
                    return att.TextString;
                }
            }

            return "";
        }

        public static Dictionary<string, string> GetAttributes(Transaction tr, AttributeCollection attCol)
        {
            var attributes = new Dictionary<string, string>();
            
            foreach (ObjectId attId in attCol)
            {
                var att = (AttributeReference)tr.GetObject(attId, OpenMode.ForRead);
                if (att.Tag != null)
                {
                    attributes.Add(att.Tag, att.TextString);
                }
            }

            return attributes;
        }

        public static bool SetAttributes(Transaction tr, AttributeCollection attCol, Dictionary<string, string> attrDict)
        {
            var result = true;
            foreach (ObjectId attId in attCol)
            {
                var att = (AttributeReference)tr.GetObject(attId, OpenMode.ForRead);
                if (att == null) continue;
                if (!attrDict.TryGetValue(att.Tag, out var valueText))
                    continue;
                if(valueText == null)
                    continue;
                att.UpgradeOpen();
                att.TextString = valueText;
                att.DowngradeOpen();
                
                if (!att.TextString.Equals(valueText))
                {
                    result = false;
                }
            }

            return result;
        }

        public static IEnumerable<AcadObjectWithAttributes> GetObjectsWithAttribute(Transaction tr, 
            IEnumerable<Entity> entities,
            string attributeTag)
        {
            var objCollection = new List<AcadObjectWithAttributes>();

            foreach (var entity in entities)
            {
                if (entity is not BlockReference br) continue;
                if (br.AttributeCollection.Count == 0) continue;
                var attrDict = GetAttributes(tr, br.AttributeCollection);
                // we need only objects contain attributeTag
                if (!attrDict.ContainsKey(attributeTag)) continue;

                var objWithAttr = new AcadObjectWithAttributes()
                {
                    Entity = entity,
                    Attributes = attrDict
                };

                objCollection.Add(objWithAttr);
            }
            return objCollection;
        }
    }
}