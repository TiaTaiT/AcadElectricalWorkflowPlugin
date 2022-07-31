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
        private static Dictionary<string, string> Aliases = new Dictionary<string, string>
        {
            { "RS485(A)", "A" },
            { "RS485(B)", "B" },
            { "RS485(GND)", "0" },
            { "+48В", "+48" },
            { "+24В", "+24" },
            { "+12В", "+12" },
            { "+5В", "+5" }
        };

        public static string GetShortAlias(string sourceDescription, string destinationDescription)
        {
            if (sourceDescription == null || destinationDescription == null)
                return string.Empty;

            var electricalChecker = new ElectricalValidation(sourceDescription, destinationDescription);
            if(!electricalChecker.IsValid)
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(electricalChecker.ErrorMessage);
            sourceDescription = electricalChecker.ShortName;
            
            /*
            if (IsShleif(sourceDescription) || IsNormalOpenClosed(sourceDescription))
                return GetShortShleifAlias(sourceDescription);
            
            var result = Aliases.TryGetValue(sourceDescription, out var alias);
            if(result)
                return alias;
            */
            return sourceDescription;
        }

        private static bool IsNormalOpenClosed(string description)
        {
            if (description.Contains("NO") || description.Contains("COM"))
                return true;

            return false;
        }

        private static string GetShortShleifAlias(string description)
        {
            string alias = "";
            if (description.Contains("ШС"))
                alias = description.Substring(description.LastIndexOf("ШС"));
            if (description.Contains("КЦ"))
                alias = description.Substring(description.LastIndexOf("КЦ"));
            if (description.Contains("NO") || description.Contains("COM") || description.Contains("NC"))
            {
                alias = TryRemoveBracketsWithContain(description);
            }
            return alias;
        }

        private static string TryRemoveBracketsWithContain(string description)
        {
            string alias;
            int charLocation = description.IndexOf("(", StringComparison.Ordinal);
            if (charLocation > 0)
            {
                alias = description.Substring(0, charLocation);
            }
            else
            {
                alias = description;
            }

            return alias;
        }

        private static bool IsShleif(string description)
        {
            if(description.Contains("ШС") || description.Contains("КЦ"))
                if(description.Contains("+") || description.Contains("-"))
                    return true;
            return false;
        }
    }
}
