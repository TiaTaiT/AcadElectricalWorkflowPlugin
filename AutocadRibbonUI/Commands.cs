using System;
using Teigha.Runtime;            // IExtensionApplication
using Bricscad.Windows;          // ComponentManager, RibbonControl, RibbonTab, etc.

[assembly: ExtensionApplication(typeof(BricscadRibbonUI.Commands))]
namespace BricscadRibbonUI
{
    public class Commands : IExtensionApplication
    {
        private RibbonControl _ribbon;
        private RibbonTab _tab;

        public void Initialize()
        {
            // Get the BricsCAD ribbon control
            _ribbon = ComponentManager.Ribbon;
            // Create and set up our custom ribbon
            CreateWorkflowTab();
        }

        public void Terminate()
        {
            // Optional: clean up by removing the tab
            if (_ribbon != null && _tab != null)
                _ribbon.Tabs.Remove(_tab);
        }

        private void CreateWorkflowTab()
        {
            // Instantiate a new tab
            _tab = new RibbonTab()
            {
                Title = "Workflow",
                Id = "rtWorkflow"
            };
            // Add the tab to BricsCAD’s ribbon
            _ribbon.Tabs.Add(_tab);

            // Create a panel for “Terminals”
            var terminalsSource = new RibbonPanelSource()
            {
                Title = "Terminals",
                Id = "rpTerminals"
            };
            var terminalsPanel = new RibbonPanel
            {
                Source = terminalsSource
            };
            _tab.Panels.Add(terminalsPanel);

            // Add a row of buttons
            var row1 = new RibbonRowPanel();
            terminalsSource.Items.Add(row1);

            // First button: Count
            var btnCount = new RibbonButton()
            {
                Id = "btnCount",
                Text = "Count",
                ShowText = true,
                Size = RibbonItemSize.Large,
                ButtonStyle = RibbonButtonStyle.LargeWithText,
                ImagePath = @"C:\Users\texvi\source\repos\TiaTaiT\AcadElectricalWorkflowPlugin\AutocadRibbonUI\Assets\10.png",
                CommandParameter = "TERMCOUNT"
            };
            row1.Items.Add(btnCount);

            // Second button: BInc
            var btnBInc = new RibbonButton()
            {
                Id = "btnBInc",
                Text = "BInc",
                ShowText = true,
                Size = RibbonItemSize.Large,
                ButtonStyle = RibbonButtonStyle.LargeWithText,
                ImagePath = @"C:\Users\texvi\source\repos\TiaTaiT\AcadElectricalWorkflowPlugin\AutocadRibbonUI\Assets\10.png",
                CommandParameter = "BINCREMENT"
            };
            row1.Items.Add(btnBInc);

            // Third button: Inc
            var btnInc = new RibbonButton()
            {
                Id = "btnInc",
                Text = "Inc",
                ShowText = true,
                Size = RibbonItemSize.Large,
                ButtonStyle = RibbonButtonStyle.LargeWithText,
                ImagePath = @"C:\Users\texvi\source\repos\TiaTaiT\AcadElectricalWorkflowPlugin\AutocadRibbonUI\Assets\10.png",
                CommandParameter = "TERMINCREMENT"
            };
            row1.Items.Add(btnInc);

            // 2. Insert a separator (vertical line)
            var separator = new RibbonSeparator() { 
                SeparatorStyle = RibbonSeparatorStyle.Line,
            };      // :contentReference[oaicite:6]{index=6}
                                                        // optional: separator.AutoDelete = true; 
            row1.Items.Add(separator);                  // :contentReference[oaicite:7]{index=7}

            // Repeat for other panels (Linker, etc.)...
            var btnGetTerminals = new RibbonButton()
            {
                Id = "btnGetTerminals",
                Text = "Insert Terminals",
                ShowText = true,
                Size = RibbonItemSize.Large,
                ButtonStyle = RibbonButtonStyle.LargeWithText,
                ImagePath = @"C:\Users\texvi\source\repos\TiaTaiT\AcadElectricalWorkflowPlugin\AutocadRibbonUI\Assets\10.png",
                CommandParameter = "GETTERMINALS"
            };
            row1.Items.Add(btnGetTerminals);

            var btnGetUiTerminals = new RibbonButton()
            {
                Id = "btnGetUiTerminals",
                Text = "UI Terminals",
                ShowText = true,
                Size = RibbonItemSize.Large,
                ButtonStyle = RibbonButtonStyle.LargeWithText,
                ImagePath = @"C:\Users\texvi\source\repos\TiaTaiT\AcadElectricalWorkflowPlugin\AutocadRibbonUI\Assets\10.png",
                CommandParameter = "GETUITERMINALS"
            };
            row1.Items.Add(btnGetUiTerminals);

            var btnDrawOrderDown = new RibbonButton()
            {
                Id = "btnDrawOrderDown",
                Text = "Order Down",
                ShowText = true,
                Size = RibbonItemSize.Large,
                ButtonStyle = RibbonButtonStyle.LargeWithText,
                ImagePath = @"C:\Users\texvi\source\repos\TiaTaiT\AcadElectricalWorkflowPlugin\AutocadRibbonUI\Assets\10.png",
                CommandParameter = "DRAWORDERDOWN"
            };
            row1.Items.Add(btnDrawOrderDown);

            row1.Items.Add(separator);

            var btnLinkMultiWires = new RibbonButton()
            {
                Id = "btnLinkMultiWires",
                Text = "Link Multiwires",
                ShowText = true,
                Size = RibbonItemSize.Large,
                ButtonStyle = RibbonButtonStyle.LargeWithText,
                ImagePath = @"C:\Users\texvi\source\repos\TiaTaiT\AcadElectricalWorkflowPlugin\AutocadRibbonUI\Assets\10.png",
                CommandParameter = "LINKMULTIWIRES"
            };
            row1.Items.Add(btnLinkMultiWires);

            var btnLinkWires = new RibbonButton()
            {
                Id = "btnLinkWires",
                Text = "Link Wires",
                ShowText = true,
                Size = RibbonItemSize.Large,
                ButtonStyle = RibbonButtonStyle.LargeWithText,
                ImagePath = @"C:\Users\texvi\source\repos\TiaTaiT\AcadElectricalWorkflowPlugin\AutocadRibbonUI\Assets\10.png",
                CommandParameter = "LINKWIRES"
            };
            row1.Items.Add(btnLinkWires);

            var btnLinkPairMultiwires = new RibbonButton()
            {
                Id = "btnLinkPairMultiwires",
                Text = "Pair Multiwires",
                ShowText = true,
                Size = RibbonItemSize.Large,
                ButtonStyle = RibbonButtonStyle.LargeWithText,
                ImagePath = @"C:\Users\texvi\source\repos\TiaTaiT\AcadElectricalWorkflowPlugin\AutocadRibbonUI\Assets\10.png",
                CommandParameter = "LINKPAIRMULTIWIRES"
            };
            row1.Items.Add(btnLinkPairMultiwires);

            var btnLinkPairRemove = new RibbonButton()
            {
                Id = "btnLinkPairRemove",
                Text = "Pair Remove",
                ShowText = true,
                Size = RibbonItemSize.Large,
                ButtonStyle = RibbonButtonStyle.LargeWithText,
                ImagePath = @"C:\Users\texvi\source\repos\TiaTaiT\AcadElectricalWorkflowPlugin\AutocadRibbonUI\Assets\10.png",
                CommandParameter = "LINKPAIRREMOVE"
            };
            row1.Items.Add(btnLinkPairRemove);

            var btnLinksRemove = new RibbonButton()
            {
                Id = "btnLinksRemove",
                Text = "Links Remove",
                ShowText = true,
                Size = RibbonItemSize.Large,
                ButtonStyle = RibbonButtonStyle.LargeWithText,
                ImagePath = @"C:\Users\texvi\source\repos\TiaTaiT\AcadElectricalWorkflowPlugin\AutocadRibbonUI\Assets\10.png",
                CommandParameter = "LINKSREMOVE"
            };
            row1.Items.Add(btnLinksRemove);

            row1.Items.Add(separator);

            var btnTagsRenumber = new RibbonButton()
            {
                Id = "btnTagsRenumber",
                Text = "Tags Renumber",
                ShowText = true,
                Size = RibbonItemSize.Large,
                ButtonStyle = RibbonButtonStyle.LargeWithText,
                ImagePath = @"C:\Users\texvi\source\repos\TiaTaiT\AcadElectricalWorkflowPlugin\AutocadRibbonUI\Assets\10.png",
                CommandParameter = "TAGSRENUMBER"
            };
            row1.Items.Add(btnLinksRemove);
        }
    }
}
