using System;
using System.Collections.Generic;
using AutocadTerminalsManager.Model;
using Autodesk.AutoCAD.DatabaseServices;

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
        public static string GetAttributeValue(AttributeCollection attCol, string tagName)
        {
            foreach (ObjectId attId in attCol)
            {
                
                var att = (AttributeReference)attId.GetObject(OpenMode.ForRead, false);
                if (att.Tag.Equals(tagName))
                {
                    return att.TextString;
                }
            }
            return "";
        }

        public static string GetAttributeValueFromBlock(Transaction tr, ObjectId objectId, string tagName)
        {
            var attrColl = GetAttributeCollection(tr, objectId);

            return GetAttributeValue(attrColl, tagName);
        }

        private static AttributeCollection GetAttributeCollection(Transaction tr, ObjectId objectId)
        {
            try
            {
                var br = (BlockReference)tr.GetObject(objectId, OpenMode.ForRead, false, true);
                //var obj = objectId.GetObject(OpenMode.ForRead, false);
                

                var attrColl = /*((BlockReference)obj)*/br.AttributeCollection;

                return attrColl;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Get attributes from AttributeCollection
        /// </summary>
        /// <param name="tr">Autocad open transaction</param>
        /// <param name="attCol">Attributes collection</param>
        /// <returns>Dictionary(AttributeTag, AttributeValue)</returns>
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

        /// <summary>
        /// Set attributes from Dictionary to AttributeCollection in open Autocad transaction
        /// </summary>
        /// <param name="tr">Open autocad transaction</param>
        /// <param name="attCol">Transit attribute collection for replacing attribute</param>
        /// <param name="attrDict">Dictionary(AttributeTag, AttributeValue) with attributes to which
        /// the values from the transit collection (AttributeCollection) will be changed.  </param>
        /// <returns></returns>
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

        public static IEnumerable<ObjectId> GetObjectsWithAttribute(Database db, Transaction tr, string attributeTag, string attributeValue)
        {
            //using var tr = db.TransactionManager.StartOpenCloseTransaction();
            BlockTableRecord btRecord = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead);
            foreach (ObjectId id in btRecord)
            {
                Entity entity = (Entity)tr.GetObject(id, OpenMode.ForRead);

                if (id.IsNull || id.IsErased || id.IsEffectivelyErased || !id.IsValid) continue;
                if (entity is not BlockReference br) continue;
                if (br.AttributeCollection.Count == 0) continue;
                
                var attrDict = GetAttributes(tr, br.AttributeCollection);
                // we need only objects contain attributeTag
                if (!attrDict.ContainsKey(attributeTag)) continue;
                // then we need only objects contain attributeValue
                attrDict.TryGetValue(attributeTag, out var tagValue);
                if (tagValue == null || !tagValue.Equals(attributeValue)) continue;
                
                yield return entity.Id;
            }
        }

        public static bool SetBlockAttributes(Transaction tr, ObjectId objectId, Dictionary<string, string> attributes)
        {
            var attrColl = GetAttributeCollection(tr, objectId);

            return(SetAttributes(tr, attrColl, attributes));
        }

        /// <summary>
        /// Get from the list of entities only those that contain the given attribute.
        /// This method does not return the entities themselves, but the AcadObjectWithAttributes.
        /// This type contains only a reference to the entity and the fields required for operation. 
        /// </summary>
        /// <param name="tr">open Autcad transaction</param>
        /// <param name="entities">Collection with entities</param>
        /// <param name="attributeTag">Attribute name for search</param>
        /// <returns>Fake entities with given attribute</returns>
        public static IEnumerable<AcadObjectWithAttributes> GetObjectsWithAttribute(Transaction tr, IEnumerable<Entity> entities, string attributeTag)
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