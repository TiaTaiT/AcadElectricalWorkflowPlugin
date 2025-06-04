using Autodesk.AutoCAD.Ribbon;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutocadRibbonUI
{
    public class Commands : IExtensionApplication
    {
        private RibbonControl _ribbon;
        private WorkFlowRibbonTab _tab;

        public RibbonCommandHandler ribbonCmdHandler { get; set; }
        public string SAMPLERIBBONSTABID { get => "Workflow"; }

        public void Initialize()
        {
            //TODO: add code to run when the ExtApp initializes. Here are a few examples:
            //          Checking some host information like build #, a patch or a particular Arx/Dbx/Dll;
            //          Creating/Opening some files to use in the whole life of the assembly, e.g. logs;
            //          Adding some ribbon tabs, panels, and/or buttons, when necessary;
            //          Loading some dependents explicitly which are not taken care of automatically;
            //          Subscribing to some events which are important for the whole session;
            //          Etc.
            if (ComponentManager.Ribbon == null)
            {
                ComponentManager.ItemInitialized += this.RibbonCompInitialized;
            }
            else
            {
                this.ApplicationSetup();
            }
        }

        private void ApplicationSetup()
        {
            SetupRibbon();
            RibbonServices.RibbonPaletteSet.WorkspaceLoaded += (this.RibbonPaletteLoaded);
            RibbonServices.RibbonPaletteSet.WorkspaceUnloading += (this.RibbonPaletteUnloaded);
        }

        private void SetupRibbon()
        {
            // Create a RibbonTab using the resourceDictionary
            _ribbon = ComponentManager.Ribbon;
            _tab = new WorkFlowRibbonTab
            {
                Id = "MyTab_001"
            };
            _ribbon.Tabs.Add(_tab);

            ribbonCmdHandler = new RibbonCommandHandler();
            for (var i = 0; i < _tab.Panels.Count(); i++)
            {
                IterateItems(i);
            }
        }

        private void IterateItems(int i)
        {
            foreach (var item in _tab.Panels[i].Source.Items)
            {
                var subItems = GetPropValue(item, "Items");
                if (subItems != null)
                {
                    foreach (var subItem in (IEnumerable<object>)subItems)
                    {
                        if (subItem is RibbonButton button)
                        {
                            button.CommandHandler = ribbonCmdHandler;
                        }
                    }

                }
                if (item is RibbonButton button1)
                {
                    button1.CommandHandler = ribbonCmdHandler;
                }
            }
        }

        public static object GetPropValue(object src, string propName)
        {
            try
            {
                return src.GetType().GetProperty(propName).GetValue(src, null);
            }
            catch
            {
                return null;
            }

        }

        private void RibbonPaletteLoaded(object sender, EventArgs e)
        {
            SetupRibbon();
        }

        private void RibbonPaletteUnloaded(object sender, EventArgs e)
        {
            RibbonServices.RibbonPaletteSet.RibbonControl.Tabs.Remove(
                RibbonServices.RibbonPaletteSet.RibbonControl.FindTab(SAMPLERIBBONSTABID));
        }

        private void RibbonCompInitialized(object sender, RibbonItemEventArgs e)
        {
            if (ComponentManager.Ribbon != null)
            {
                ComponentManager.ItemInitialized -= this.RibbonCompInitialized;
                this.ApplicationSetup();
            }
        }

        public void Terminate()
        {
            // _ribbon.Tabs.Remove(_tab);

        }
    }
}