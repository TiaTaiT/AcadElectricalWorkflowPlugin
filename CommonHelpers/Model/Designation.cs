
namespace CommonHelpers.Models
{
    public class Designation
    {
        /// <summary>
        /// Location code. For example, in designation "7.1ШСi12.3+" location code is "7.1".
        /// Optional.
        /// </summary>
        public string Location { get; set; } = "";

        /// <summary>
        /// Signal appointment. For example, in designation "7.1ШСi12.3+" signal appointment is "ШС" or
        /// in designation "+12В1" signal appointment is "+12В"
        /// Required.
        /// </summary>
        public string Appointment { get; set; } = "";

        /// <summary>
        /// Spark protection on signal/power circuits. For example, in designation "7.1ШСi12.3+" spark protection is "i".
        /// Optional.
        /// </summary>
        public string SparkProtection { get; set; } = "";

        /// <summary>
        /// Signal number. For example, in designation "7.1ШСi12.3+" signal number is "12.3" or
        /// in designation "+12В1" signal number is "1"
        /// Optional
        /// </summary>
        public string Number { get; set; } = "";

        /// <summary>
        /// Last part of the full designation. For example, in designation "7.1ШСi12.3+" Suffix is "+".
        /// </summary>
        public string Suffix { get; set; } = "";

        /// <summary>
        /// Surge protection on signal/power circuits (Round brackets). For example, (7.1ШС12+), (1ШС1-)
        /// </summary>
        public bool SurgeProtection { get; set; }

        /// <summary>
        /// Lower voltage
        /// </summary>
        public int LowerVoltage { get; set; }

        /// <summary>
        /// Upper voltage
        /// </summary>
        public int UpperVoltage { get; set; }

        public NetTypes ElectricalType { get; set; } = NetTypes.Unknown;

        public bool IsVoltageRange { get => UpperVoltage != LowerVoltage; }

        public override string ToString()
        {
            var designation = Location + Appointment + SparkProtection + Number + Suffix;

            if (SurgeProtection)
                return "(" + designation + ")";
            return designation;
        }

        public override bool Equals(object obj)
        {

            if ((obj == null) && (this != null))
            {
                return false;
            }
            var testObj = (HalfWireDesignation)obj;

            return Location.Equals(testObj.Location) &&
                   Appointment.Equals(testObj.Appointment) &&
                   SparkProtection.Equals(testObj.SparkProtection) &&
                   Number.Equals(testObj.Number) &&
                   Suffix.Equals(testObj.Suffix) &&
                   LowerVoltage == testObj.LowerVoltage &&
                   UpperVoltage == testObj.UpperVoltage &&
                   ElectricalType == testObj.ElectricalType &&
                   SurgeProtection == testObj.SurgeProtection;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 0;

                // String properties
                hashCode = (hashCode * 397) ^ (Location != null ? Location.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Appointment != null ? Location.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SparkProtection != null ? Appointment.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Number != null ? SparkProtection.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Suffix != null ? Number.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ElectricalType != null ? Suffix.GetHashCode() : 0);

                // int properties
                hashCode = (hashCode * 397) ^ LowerVoltage;
                hashCode = (hashCode * 397) ^ UpperVoltage;


                return hashCode;
            }
        }
    }
}
