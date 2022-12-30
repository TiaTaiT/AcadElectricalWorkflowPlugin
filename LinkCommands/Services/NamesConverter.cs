using LinkCommands.Interfaces;
using LinkCommands.Models;
using System.Collections.Generic;

namespace LinkCommands.Services
{
    public class NamesConverter : INamesConverter
    {
        private const string _positiveSign = "+";
        private readonly List<string> _voltageSigns = new() { "В", "V" };

        public string GetShortName(HalfWireDesignation sourceDesignation, HalfWireDesignation destDesignation)
        {
            if (sourceDesignation.ElectricalType == NetTypes.Unknown &&
                destDesignation.ElectricalType == NetTypes.PowerPositive)
            {
                return _positiveSign +
                       destDesignation.LowerVoltage +
                       destDesignation.Appointment +
                       destDesignation.Number +
                       destDesignation.Suffix;
            }

            if (sourceDesignation.ElectricalType == NetTypes.Unknown &&
                destDesignation.ElectricalType == NetTypes.PowerNegative)
            {
                return destDesignation.LowerVoltage +
                       destDesignation.Appointment +
                       destDesignation.Number +
                       destDesignation.Suffix;
            }

            if (destDesignation.ElectricalType == NetTypes.Unknown &&
                sourceDesignation.ElectricalType == NetTypes.PowerPositive)
            {
                return _positiveSign +
                       sourceDesignation.LowerVoltage +
                       sourceDesignation.Appointment +
                       sourceDesignation.Number +
                       sourceDesignation.Suffix;
            }

            if (destDesignation.ElectricalType == NetTypes.Unknown &&
                sourceDesignation.ElectricalType == NetTypes.PowerNegative)
            {
                return sourceDesignation.LowerVoltage +
                       sourceDesignation.Appointment +
                       sourceDesignation.Number +
                       sourceDesignation.Suffix;
            }

            if (sourceDesignation.ElectricalType == NetTypes.PowerPositive ||
                destDesignation.ElectricalType == NetTypes.PowerPositive)
            {
                return _positiveSign + sourceDesignation.LowerVoltage + sourceDesignation.Appointment + sourceDesignation.Number;
            }

            if (sourceDesignation.ElectricalType == NetTypes.PowerNegative ||
                destDesignation.ElectricalType == NetTypes.PowerNegative)
            {
                if( _voltageSigns.Contains(sourceDesignation.Appointment))
                    return sourceDesignation.LowerVoltage + 
                           sourceDesignation.Appointment + 
                           sourceDesignation.Number;
                return sourceDesignation.Appointment + destDesignation.Number;
            }

            if (sourceDesignation.IsShleif)
            {
                return sourceDesignation.Appointment + sourceDesignation.Number + sourceDesignation.Suffix;
            }

            if (destDesignation.IsShleif)
            {
                return destDesignation.Appointment + destDesignation.Number + destDesignation.Suffix;
            }

            return sourceDesignation.Appointment + sourceDesignation.Number + sourceDesignation.Suffix;
        }
    }
}
