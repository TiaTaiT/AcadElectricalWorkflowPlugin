using AutocadCommands.Services;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using CommonHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if(_startNumberStr.Equals("Y"))
                return true;

            return false;
            #endregion
        }

        public override void Run()
        {
            
            var symbolNames = LinkSymbolNameResolver.GetAllNames();

            var linkIds = GetObjectsUtils.GetBlockIdsByNames(_db, symbolNames);
            _doc.LockDocument();
            BlockHelper.EraseEntitiesByIds(_db, linkIds);
            
            
        }
    }
}
