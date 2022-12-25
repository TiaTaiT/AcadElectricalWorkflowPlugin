using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkCommands.Models
{
    public class HalfWireDesignation : Designation
    {
        public bool IsInRangeVoltage(int voltage)
        {
            return (voltage >= LowerVoltage) && (voltage <= UpperVoltage);
        }

        public bool IsPower
        {
            get => (ElectricalType == NetTypes.PowerPositive) ||
                   (ElectricalType == NetTypes.PowerPositive);
        }

        public bool IsShleif
        {
            get => (ElectricalType == NetTypes.ShleifPositive) ||
                   (ElectricalType == NetTypes.ShleifNegative);
        }

        public bool IsDpls
        {
            get => (ElectricalType == NetTypes.DplsPositive) ||
                   (ElectricalType == NetTypes.DplsNegative);
        }

        public bool IsLadogaRs
        {
            get => (ElectricalType == NetTypes.LadogaRsPositive) ||
                   (ElectricalType == NetTypes.LadogaRsNegative);
        }

        public bool IsRs485
        {
            get => (ElectricalType == NetTypes.Rs485A ||
                    ElectricalType == NetTypes.Rs485B);
        }
    }
}
