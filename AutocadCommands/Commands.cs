using Autodesk.AutoCAD.Runtime;
using System;
using System.IO;
using System.Net.Mime;
using AutocadCommands.Services;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutocadCommands
{
    public class Commands
    {
        private readonly string _configFilePath = Environment.CurrentDirectory + "\\config.txt";
        private ConfigProvider _configProvider;

        // Renumber terminals TERMXX properties.
        [CommandMethod("TERMCOUNT")]
        public void TerminalsRenumber()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            var tRenumber = new TermRenumber(ed, doc, db);
            tRenumber.Run();
        }

        // Change the color of the terminals according to their purpose. 
        [CommandMethod("TERMCOLOR", CommandFlags.Modal)]
        public void TerminalsColorAutoReplace()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            if (!File.Exists(_configFilePath))
            {
                ed.WriteMessage("Config file (" + _configFilePath + ") not found.");
                return;
            }

            _configProvider = new ConfigProvider(_configFilePath);
            var tColorReplacer = new TermColorReplacer(ed, doc, db, _configProvider);
            tColorReplacer.Run();

        }

        // Advanced DESC1 increment of terminals
        [CommandMethod("TERMINCREMENT")]
        public void TerminalsDescriptionIncrement()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            var tDescInc = new TermDescriptionIncrement(ed, doc, db);
            tDescInc.Run();
        }

        // Add prefix to DESC1 of terminals
        [CommandMethod("TERMADDPREFIX")]
        public void TerminalsAddDescriptionPrefix()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            var tDescInc = new TermAddDescriptionPrefix(ed, doc, db);
            tDescInc.Run();
        }

        // Add prefix to DESC1 of terminals
        [CommandMethod("TERMFINDREPLACE")]
        public void TerminalsFindAndReplace()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            var tDescInc = new TermFindAndReplace(ed, doc, db);
            tDescInc.Run();
        }

        // Add prefix to DESC1 of terminals
        [CommandMethod("TERMSETATTRIBUTE")]
        public void SetBlocksAttribute()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            var blockAttributeReplacer = new BlocksAttributeReplacer(ed, doc, db);
            blockAttributeReplacer.Run();
        }
    }
}
