using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace LinkCommands.Services
{
    internal static class NamesConverter
    {
        public static string GetShortAlias(string sourceDescription, string destinationDescription)
        {
            if (sourceDescription == null || destinationDescription == null)
                return string.Empty;

            var electricalChecker = new ElectricalValidation(sourceDescription, destinationDescription);
            if(!electricalChecker.IsValid)
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(electricalChecker.ErrorMessage);
            sourceDescription = electricalChecker.ShortName;
            
            return sourceDescription;
        }
    }
}
