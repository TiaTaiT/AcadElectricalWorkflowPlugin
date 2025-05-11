using AutocadCommands.Helpers;
using AutocadCommands.Models;
using System;
using System.Collections.Generic;
using System.Linq;

using Teigha.DatabaseServices;


namespace AutocadCommands.Services
{
    public class TerminalsHelper
    {
        private static Dictionary<string, string> GetAttributesDictionary(Terminal terminal)
        {
            return new()
            {
                { "DESC1", terminal.Description1 },
                { "DESC2", terminal.Description2 },
                { "DESC3", terminal.Description3 },
                { "TERM01", terminal.TerminalNumber.ToString() },
                { "LINKTERM", terminal.UniqId },
                { "CATDESC", terminal.CatalogDescription },
                { "CAT", terminal.Catalog },
                { "MFG", terminal.Manufacturing },
                { "TAGSTRIP", terminal.TagStrip },
                { "CABLEDESIGNATION", terminal.Cable }
            };
        }

        public static void SetTerminals(Transaction tr, ObjectIdCollection objIds, IReadOnlyCollection<Terminal> terminals)
        {
            foreach (ObjectId blockId in objIds)
            {

                var blockRef = (BlockReference)tr.GetObject(blockId, OpenMode.ForRead);

                var attCol = blockRef.AttributeCollection;

                var guid = AttributeHelper.GetAttributeValue(attCol, "LINKTERM");
                var terminal = FindTerminal(terminals, guid);

                var attrDict = GetAttributesDictionary(terminal);
                AttributeHelper.SetAttributes(tr, attCol, attrDict);
            }
        }

        public static void SetTerminal(Transaction tr, ObjectId blockId, Terminal terminal)
        {
            var blockRef = (BlockReference)tr.GetObject(blockId, OpenMode.ForRead);

            var attCol = blockRef.AttributeCollection;

            var attrDict = GetAttributesDictionary(terminal);
            AttributeHelper.SetAttributes(tr, attCol, attrDict);
        }

        public static Terminal FindTerminal(IEnumerable<Terminal> terminals, string guid)
        {
            foreach (var terminal in terminals)
            {
                if (terminal.UniqId.Equals(guid))
                    return terminal;
            }

            return null;
        }

        public static List<Terminal> GetTerminals(Transaction tr, ObjectIdCollection blockIds, bool newGuid)
        {
            var terminals = new List<Terminal>();

            foreach (ObjectId blockId in blockIds)
            {
                var blockRef = (BlockReference)tr.GetObject(blockId, OpenMode.ForRead);
                var btr = (BlockTableRecord)tr.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead);

                var terminal = new Terminal { Name = btr.Name };

                btr.Dispose();

                terminal.X = blockRef.Position.X;
                terminal.Y = blockRef.Position.Y;
                terminal.Z = blockRef.Position.Z;

                var attCol = blockRef.AttributeCollection;

                foreach (ObjectId attId in attCol)
                {
                    var att = (AttributeReference)tr.GetObject(attId, OpenMode.ForRead);
                    att.UpgradeOpen();

                    switch (att.Tag)
                    {
                        case "DESC1":
                            terminal.Description1 = att.TextString;
                            break;

                        case "DESC2":
                            terminal.Description2 = att.TextString;
                            break;

                        case "DESC3":
                            terminal.Description3 = att.TextString;
                            //att.TextString = "!!!";
                            break;

                        case "TERM01":
                            int.TryParse(att.TextString, out var numb);
                            terminal.TerminalNumber = numb;
                            break;

                        case "LINKTERM":
                            if (!newGuid)
                            {
                                if (terminals.Any(x => x.UniqId.Contains(att.TextString)))
                                {
                                    att.TextString = Guid.NewGuid().ToString();
                                }
                            }
                            else
                            {
                                att.TextString = Guid.NewGuid().ToString();
                            }
                            terminal.UniqId = att.TextString;
                            break;

                        case "CATDESC":
                            terminal.CatalogDescription = att.TextString;
                            break;

                        case "CAT":
                            terminal.Catalog = att.TextString;
                            break;

                        case "MFG":
                            terminal.Manufacturing = att.TextString;
                            break;

                        case "TAGSTRIP":
                            terminal.TagStrip = att.TextString;
                            break;

                        case "CABLEDESIGNATION":
                            terminal.Cable = att.TextString;
                            break;
                    }
                }

                terminals.Add(terminal);

            }

            return terminals;
        }

        public static Terminal GetTerminal(Transaction tr, ObjectId blockId)
        {
            var terminal = new Terminal();

            var blockRef = (BlockReference)tr.GetObject(blockId, OpenMode.ForRead);
            var btr = (BlockTableRecord)tr.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead);

            terminal.Name = btr.Name;

            btr.Dispose();

            terminal.X = blockRef.Position.X;
            terminal.Y = blockRef.Position.Y;
            terminal.Z = blockRef.Position.Z;

            var attCol = blockRef.AttributeCollection;

            foreach (ObjectId attId in attCol)
            {
                var att = (AttributeReference)tr.GetObject(attId, OpenMode.ForRead);
                att.UpgradeOpen();

                switch (att.Tag)
                {
                    case "DESC1":
                        terminal.Description1 = att.TextString;
                        break;
                    case "DESC2":
                        terminal.Description2 = att.TextString;
                        break;
                    case "DESC3":
                        terminal.Description3 = att.TextString;
                        //att.TextString = "!!!";
                        break;
                    case "TERM01":
                        int.TryParse(att.TextString, out var numb);
                        terminal.TerminalNumber = numb;
                        break;
                    case "LINKTERM":
                        terminal.UniqId = att.TextString;
                        break;
                    case "CATDESC":
                        terminal.CatalogDescription = att.TextString;
                        break;
                    case "CAT":
                        terminal.Catalog = att.TextString;
                        break;
                    case "MFG":
                        terminal.Manufacturing = att.TextString;
                        break;
                    case "TAGSTRIP":
                        terminal.TagStrip = att.TextString;
                        break;
                    case "CABLEDESIGNATION":
                        terminal.Cable = att.TextString;
                        break;
                }
            }

            return terminal;
        }
    }
}