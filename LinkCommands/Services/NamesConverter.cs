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

            var electricalValidator = new ElectricalValidation();
            var validationResult = electricalValidator.ValidateWire(sourceDescription, destinationDescription);
            if(!validationResult)
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(electricalValidator.ErrorMessage);
            sourceDescription = electricalValidator.ShortName;
            
            return sourceDescription;
        }
    }
}
