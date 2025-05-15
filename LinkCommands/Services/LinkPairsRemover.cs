using AutocadCommands.Services;
using CommonHelpers;

using Bricscad.ApplicationServices;
using Bricscad.EditorInput;

namespace LinkCommands.Services
{
    public class LinkPairsRemover : CommandPrototype
    {
        public LinkPairsRemover(Document doc) : base(doc)
        {
        }

        public override bool Init()
        {
            #region Dialog with user

            var promptResult = _ed.GetString("\nDo you really want to remove all pair links? <Y/N>: ");
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

            var symbolNames = LinkSymbolNameResolver.GetAllNames();

            var linkIds = GetObjectsUtils.GetBlockIdsByNames(_db, _tr, symbolNames);
            _doc.LockDocument();
            BlockHelper.EraseEntitiesByIds(_db, _tr, linkIds);


        }
    }
}
