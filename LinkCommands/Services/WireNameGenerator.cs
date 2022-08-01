using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Autodesk.AutoCAD.DatabaseServices.RenderGlobal;

namespace LinkCommands.Services
{
    internal static class WireNameGenerator
    {
        public enum SignalType
        {
            Rs485 = 0,
            Shleif = 1,
            KC = 2,
            Power = 3,

        }
        public static string GetShortWireName(string source, string destination, SignalType signalType)
        {
            if(signalType == SignalType.Rs485)
            {
                if (source.Contains("A") || destination.Contains("A"))
                    return "A";
                if (source.Contains("B") || destination.Contains("B"))
                    return "B";
                if (source.Contains("GND") || destination.Contains("GND") || source.Contains("0В") || destination.Contains("0В"))
                    return "0В";
            }

            if (signalType == SignalType.Shleif)
            {
                string alias = "";
                if (source.Contains("ШС") || !destination.Contains("ШС"))
                    alias = source.Substring(source.LastIndexOf("ШС"));
                if (!source.Contains("ШС") || destination.Contains("ШС"))
                    alias = destination.Substring(destination.LastIndexOf("ШС"));

                if (source.Contains("КЦ") || destination.Contains("КЦ"))
                    alias = source.Substring(source.LastIndexOf("КЦ"));
                if (source.Contains("КЦ") || destination.Contains("КЦ"))
                    alias = destination.Substring(destination.LastIndexOf("КЦ"));

                return alias;
            }
            return "";
        }
    }
}
