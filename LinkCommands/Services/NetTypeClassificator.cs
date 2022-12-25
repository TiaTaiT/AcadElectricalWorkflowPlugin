using LinkCommands.Models;
using System.Collections.Generic;
using System.Linq;

namespace LinkCommands.Services
{
    public class NetTypeClassificator
    {
        private static readonly IEnumerable<string> _shleifs = new List<string> { "ШС", "КЦ" };
        private static readonly IEnumerable<string> _power = new List<string> { "В", "GND", "ПИ", "-U", "-U" };
        private static readonly IEnumerable<string> _powerPositive = new List<string> { "ПИ+", "ПИ1+", "ПИ2+", "ПИ3+", "ПИ4+", "ПИ5+", "ПИ6+", "ПИ7+", "ПИ8+", "+U1", "+U2", "(12-24)В", "(20-75)В" };
        private static readonly IEnumerable<string> _rs485A = new List<string> { "RS485A", "RS485(A)", "A", "A1", "A2", "A3", "A4", "ЛС+", "2", "9" };
        private static readonly IEnumerable<string> _rs485B = new List<string> { "RS485B", "RS485(B)", "B", "B1", "B2", "B3", "B4", "ЛС-", "5", "10" };
        private static readonly IEnumerable<string> _rs485Gnd = new List<string> { "RS485GND", "RS485(GND)", "C", "C1", "C2", "C3", "C4", "3", "4", "8" };
        private static readonly IEnumerable<string> _dpls = new List<string> { "ДПЛС-", "ДПЛС-", "ДПЛС" };
        private static readonly IEnumerable<string> _dplsNegative = new List<string> { "ДПЛС-1-", "ДПЛС-2-", "ДПЛС1-", "ДПЛС2-" };
        private static readonly IEnumerable<string> _relay = new List<string> { "NO", "COM", "NC", "K" };

        public NetTypeClassificator() { }

        internal static NetTypes GetNetType(string appointment, string suffix)
        {
            if (_shleifs.Contains(appointment))
            {
                if (suffix.Equals("+"))
                    return NetTypes.ShleifPositive;
                return NetTypes.ShleifNegative;
            }
            if (_power.Contains(appointment))
            {
                if (suffix.Equals("+"))
                    return NetTypes.PowerPositive;
                return NetTypes.PowerNegative;
            }
            if (_rs485A.Contains(appointment))
            {
                return NetTypes.Rs485A;
            }
            if (_rs485B.Contains(appointment))
            {
                return NetTypes.Rs485B;
            }
            if (_rs485Gnd.Contains(appointment))
            {
                return NetTypes.Rs485B;
            }
            if (_dpls.Contains(appointment))
            {
                if (suffix.Equals("+"))
                    return NetTypes.DplsPositive;
                return NetTypes.DplsNegative;
            }
            if (_relay.Contains(appointment))
            {
                return NetTypes.Relay;
            }
            return NetTypes.Unknown;
        }
    }
}
