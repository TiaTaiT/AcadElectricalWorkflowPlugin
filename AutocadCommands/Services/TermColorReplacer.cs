using AutocadCommands.Helpers;
using CommonHelpers;
using System;
using System.Collections.Generic;

namespace AutocadCommands.Services
{
    public class TermColorReplacer : CommandPrototype
    {

        private readonly ConfigProvider _configProvider;
        private PromptSelectionResult _selectedBlocks;

        public TermColorReplacer(Document doc, ConfigProvider configProvider) : base(doc)
        {
            _configProvider = configProvider;
        }

        public override bool Init()
        {
            #region Dialog with user

            var filter = new SelectionFilter(
                new[]
                {
                    new TypedValue(0, "INSERT"), new TypedValue(2, "*T0002_*")
                });

            var opts = new PromptSelectionOptions
            {
                MessageForAdding = "Select block references: "
            };

            //Make the selection   
            _selectedBlocks = _ed.GetSelection(opts, filter);

            return _selectedBlocks.Status == PromptStatus.OK;

            #endregion Dialog with user
        }

        public override void Run()
        {
            // Lock the document
            using var acLckDoc = _doc.LockDocument();
            var objIds = new ObjectIdCollection(_selectedBlocks.Value.GetObjectIds());

            using var acTrans = _db.TransactionManager.StartTransaction();
            foreach (ObjectId blockId in objIds)
            {
                var blockRef = (BlockReference)acTrans.GetObject(blockId, OpenMode.ForWrite);
                var btr = (BlockTableRecord)acTrans.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead);

                var attCol = blockRef.AttributeCollection;

                // Get the block name of the terminal to be replaced.
                var blockName = btr.Name;
                btr.Dispose();

                // It is necessary to select the brand(color) of the terminal in accordance with the "DESC1" attribute 
                var desc1 = AttributeHelper.GetAttributeValue(attCol, "DESC1");
                var colorName = GetNameByPurpose(blockName, desc1);
                // If the color of the terminal matches its purpose, skip it. 
                if (blockName.Equals(colorName)) continue;

                // Save the coordinates of the old terminal in order to insert a new one in its place. 
                var position = blockRef.Position;

                var terminal = TerminalsHelper.GetTerminal(acTrans, blockId);

                // Erase old terminal
                blockRef.Erase();

                // Insert the new terminal in place of the old one.
                /*var newTerminalId = BlockHelper.InsertElectricalBlock(position, colorName);
                if (newTerminalId.IsNull)
                    return;

                // Write the attributes of the old terminal to the new one. 
                TerminalsHelper.SetTerminal(acTrans, newTerminalId, terminal);
                */
            }

            acTrans.Commit();
        }

        private string GetNameByPurpose(string blockName, string desc1)
        {
            if (IsIn(_configProvider.YellowTerminals, desc1))
                return GetColorBlockName(blockName, "YELLOW");
            if (IsIn(_configProvider.BlackTerminals, desc1))
                return GetColorBlockName(blockName, "BLACK");
            if (IsIn(_configProvider.OrangeTerminals, desc1))
                return GetColorBlockName(blockName, "ORANGE");
            if (IsIn(_configProvider.WhiteTerminals, desc1))
                return GetColorBlockName(blockName, "WHITE");
            if (IsIn(_configProvider.GreenYellowTerminals, desc1))
                return GetColorBlockName(blockName, "GREEN-YELLOW");
            if (IsIn(_configProvider.RedTerminals, desc1))
                return GetColorBlockName(blockName, "RED");
            if (IsIn(_configProvider.BlueTerminals, desc1))
                return GetColorBlockName(blockName, "BLUE");
            return GetColorBlockName(blockName, "GREY");
        }

        private bool IsIn(IEnumerable<string> possibleTerminalsValue, string desc1)
        {
            foreach (var val in possibleTerminalsValue)
            {
                if (desc1.Contains(val)) return true;
            }

            return false;
        }

        private string GetColorBlockName(string oldBlockName, string color)
        {
            var terminalPrefix =
                oldBlockName.Substring(0, oldBlockName.LastIndexOf("_", StringComparison.Ordinal) + 1);
            return terminalPrefix + color;
        }


    }
}