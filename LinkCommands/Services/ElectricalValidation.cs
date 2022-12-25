using LinkCommands.Interfaces;
using LinkCommands.Models;

namespace LinkCommands.Services
{
    public class ElectricalValidation
    {
        public string ErrorMessage { get; set; } = "";

        public bool IsValid { get; set; } = true;

        public string ShortName { get; set; } = "";

        public bool ValidationParameterIsTerminal = false;
        private readonly IDesignationParser _designationParser;
        private readonly INamesConverter _namesConverter;

        public ElectricalValidation(IDesignationParser designationParser, INamesConverter namesConverter)
        {
            _designationParser = designationParser;
            _namesConverter = namesConverter;
        }

        public bool IsConnectionValid(string sourceString, string destinationString)
        {
            if (sourceString == null || destinationString == null)
                return false;

            var source = _designationParser.GetDesignation(sourceString);
            var destination = _designationParser.GetDesignation(destinationString);

            var compatible = AreTypesCompatible(source.ElectricalType, destination.ElectricalType);

            if (compatible)
            {
                ShortName = _namesConverter.GetShortName(source, destination);
                if (ValidationParameterIsTerminal)
                {
                    return IsComponentTerminalLinkValid(source, destination);
                }

                if (source.IsShleif && destination.IsShleif)
                {
                    return source.Number.Equals(destination.Number);
                }

                return true;
            }

            return false;
        }

        private bool IsComponentTerminalLinkValid(HalfWireDesignation source, HalfWireDesignation destination)
        {
            if (AreEqualsTypes(source, destination) == false)
                return false;

            if (source.IsPower && destination.IsPower)
            {
                return IsVoltageValid(source, destination);
            }

            if (source.IsRs485 && destination.IsRs485)
            {
                return source.Appointment.Equals(destination.Appointment);
            }

            if (IsExactMatch(source, destination))
                return true;

            return false;
        }

        private static bool IsExactMatch(HalfWireDesignation source, HalfWireDesignation destination)
        {
            return source.Appointment.Equals(destination.Appointment) &&
                                   source.Number.Equals(destination.Number) &&
                                   source.Suffix.Equals(destination.Suffix);
        }

        private bool AreEqualsTypes(HalfWireDesignation source, HalfWireDesignation destination)
        {
            return source.ElectricalType == destination.ElectricalType;
        }

        private static bool IsVoltageValid(HalfWireDesignation source, HalfWireDesignation destination)
        {
            if (source.IsVoltageRange && destination.IsVoltageRange)
            {
                return false;
            }
            if (source.IsVoltageRange)
            {
                if (source.IsInRangeVoltage(destination.LowerVoltage))
                {
                    return true;
                }
            }
            if (destination.IsVoltageRange)
            {
                if (destination.IsInRangeVoltage(source.LowerVoltage))
                {
                    return true;
                }
            }
            return false;
        }

        private bool AreTypesCompatible(NetTypes netType1, NetTypes netType2)
        {
            if (netType1 == netType2)
                return true;
            if (netType1 == NetTypes.Unknown || netType2 == NetTypes.Unknown)
                return true;
            if ((netType1 == NetTypes.ShleifNegative || netType1 == NetTypes.ShleifPositive) && netType2 == NetTypes.Relay)
                return true;
            if (netType1 == NetTypes.Relay && (netType2 == NetTypes.ShleifNegative || netType2 == NetTypes.ShleifPositive))
                return true;

            return false;
        }
    }
}