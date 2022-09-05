using Autodesk.AutoCAD.GraphicsInterface;
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
                var kcStr = "КЦ";
                if (source.Contains(shleifStr) && !destination.Contains(shleifStr))
                    return shleifStr + TextAfter(source, shleifStr);
                if (!source.Contains(shleifStr) && destination.Contains(shleifStr))
                    return shleifStr + TextAfter(destination, shleifStr);
                if (source.Contains(shleifStr) && destination.Contains(shleifStr))
                    return shleifStr + TextAfter(source, shleifStr);

                if (source.Contains(kcStr) && !destination.Contains(kcStr))
                    return kcStr + TextAfter(source, kcStr);
                if (!source.Contains(kcStr) && destination.Contains(kcStr))
                    return kcStr + TextAfter(destination, kcStr);
                if (source.Contains(kcStr) && destination.Contains(kcStr))
                    return kcStr + TextAfter(source, kcStr);
            }
            return "";
        }
        public static string TextAfter(this string value, string search)
        {
            return value.Substring(value.IndexOf(search) + search.Length);
        }
    }
}
