using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutocadRibbonUI
{
    public class Commands : IExtensionApplication
    {
        private RibbonControl _ribbon;        
        private WorkFlowRibbonTab _tab;
        
        public RibbonCommandHandler ribbonCmdHandler { get; set; }

        public void Initialize()
        {
            // Create a RibbonTab using the resourceDictionary

            _ribbon = ComponentManager.Ribbon;
            _tab = new WorkFlowRibbonTab();
            _ribbon.Tabs.Add(_tab);
            ribbonCmdHandler = new RibbonCommandHandler();
            foreach (var item in _tab.Panels[0].Source.Items)
            {
                if (item is RibbonButton button)
                {
                    button.CommandHandler = ribbonCmdHandler;
                }
            }
        }
       
        public void Terminate()
        {
            _ribbon.Tabs.Remove(_tab);
            
        }        
    }
}
