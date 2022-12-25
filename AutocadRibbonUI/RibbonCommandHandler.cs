using Autodesk.Windows;
using System;
using System.Windows.Input;
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