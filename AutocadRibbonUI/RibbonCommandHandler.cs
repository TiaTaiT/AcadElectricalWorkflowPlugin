using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AutocadRibbonUI
{
    public class RibbonCommandHandler : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is RibbonButton button1)
            {
                //var doc = Application.DocumentManager.CurrentDocument;
                var cmdString = button1.CommandParameter + " ";
                if (Application.DocumentManager != null)
                { 
                    Application.DocumentManager.MdiActiveDocument.SendStringToExecute
                        (cmdString,
                        true,
                        false,
                        true);
                }
            }
            

        }

        public event EventHandler CanExecuteChanged;
    }
}