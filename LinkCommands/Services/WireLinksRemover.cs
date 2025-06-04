﻿using AutocadCommands.Services;
using CommonHelpers;

namespace LinkCommands.Services
{
    public class WireLinksRemover : CommandPrototype
    {
        public WireLinksRemover(Document doc) : base(doc)
        {
        }

        public override bool Init()
        {
            #region Dialog with user

            var promptResult = _ed.GetString("\nDo you really want to remove all wires links? <Y/N>: ");
            if (promptResult.Status != PromptStatus.OK)
                return false;

            // If user pass input and press enter
            var _startNumberStr = promptResult.StringResult;
            if (string.IsNullOrEmpty(_startNumberStr))
                return false;

            if (_startNumberStr.Equals("Y"))
                return true;

            return false;
            #endregion
        }

        public override void Run()
        {
            var symbolNames = WiresLinkNameResolver.GetAllNames();

            var linkIds = GetObjectsUtils.GetBlockIdsByNames(_db, _tr, symbolNames);
            _doc.LockDocument();
            BlockHelper.EraseEntitiesByIds(_db, _tr, linkIds);
        }
    }
}
