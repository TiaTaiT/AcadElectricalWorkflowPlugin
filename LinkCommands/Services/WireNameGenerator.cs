using Autodesk.AutoCAD.GraphicsInterface;
using CommonHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Autodesk.AutoCAD.DatabaseServices.RenderGlobal;

namespace LinkCommands.Services
{
    public static class WireNameGenerator
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
                var shleifStr = "ШС";

                if (source.Contains(shleifStr) && !destination.Contains(shleifStr))
                    return shleifStr + TextAfter(source, shleifStr);
                if (!source.Contains(shleifStr) && destination.Contains(shleifStr))
                    return shleifStr + TextAfter(destination, shleifStr);
                if (source.Contains(shleifStr) && destination.Contains(shleifStr))
                    return shleifStr + TextAfter(source, shleifStr);
                if (source.Contains(shleifStr) && destination.Contains("RE"))
                    return shleifStr + TextAfter(source, shleifStr);
                if (source.Contains("RE") && destination.Contains(shleifStr))
                    return shleifStr + TextAfter(source, shleifStr);
                if (source.Contains(shleifStr) && destination.StartsWith(""))
                    return shleifStr + TextAfter(source, shleifStr);
                if (source.StartsWith("") && destination.Contains(shleifStr))
                    return shleifStr + TextAfter(source, shleifStr);
            }

            if (signalType == SignalType.KC)
            {
                var kcStr = "КЦ";

                if (source.Contains(kcStr) && !destination.Contains(kcStr))
                    return kcStr + TextAfter(source, kcStr);
                if (!source.Contains(kcStr) && destination.Contains(kcStr))
                    return kcStr + TextAfter(destination, kcStr);
                if (source.Contains(kcStr) && destination.Contains(kcStr))
                    return kcStr + TextAfter(source, kcStr);
            }

            if (signalType == SignalType.Power)
            {
                if (source.StartsWith("0В") && destination.StartsWith("0В"))
                    return "0В";
                if (source.StartsWith("GND") && destination.StartsWith("GND"))
                    return "GND";
                if (source.StartsWith("GND") && destination.StartsWith("0В"))
                    return source;
                if (source.StartsWith("0В") && destination.StartsWith("GND"))
                    return source;
                if (source.Equals("L") && destination.Equals("L"))
                    return "L";
                if (source.Equals("N") && destination.Equals("N"))
                    return "N";
                if (source.StartsWith("+") && source.EndsWith("В") && destination.StartsWith("+") && !destination.EndsWith("В"))
                    return source;
                if (source.StartsWith("+") && !source.EndsWith("В") && destination.StartsWith("+") && destination.EndsWith("В"))
                    return destination;
                if (source.StartsWith("+") && !destination.StartsWith("+"))
                    return source;
                if(!source.StartsWith("+") && destination.StartsWith("+"))
                    return destination;
                if (source.StartsWith("-ПИ") && !destination.StartsWith("-ПИ"))
                    return destination;
                if (!source.StartsWith("-ПИ") && destination.StartsWith("-ПИ"))
                    return source;
                if (source.StartsWith("-U") && !destination.StartsWith("-U"))
                    return destination;
                if (!source.StartsWith("-U") && destination.StartsWith("-U"))
                    return source;
                if (source.StartsWith("-") && !destination.StartsWith("-"))
                    return source;
                if (!source.StartsWith("-") && destination.StartsWith("-"))
                    return destination;
                if (StringUtils.IsNumeric(source) && !StringUtils.IsNumeric(destination))
                    return destination;
                if (!StringUtils.IsNumeric(source) && StringUtils.IsNumeric(destination))
                    return source;
                if (source.Equals(destination))
                    return source;
            }
            return "??";
        }
        public static string TextAfter(this string value, string search)
        {
            return value.Substring(value.IndexOf(search) + search.Length);
        }
    }
}
