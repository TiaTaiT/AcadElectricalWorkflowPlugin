using Autodesk.Windows;
using System;

namespace AutocadRibbonUI
{
    /// <summary>
    /// Interaction logic for WorkFlowRibbonTab.xaml
    /// </summary>
    public partial class WorkFlowRibbonTab : RibbonTab
    {
        public WorkFlowRibbonTab()
        {
            Id = Guid.NewGuid().ToString();
            InitializeComponent();
        }
    }
    
}
