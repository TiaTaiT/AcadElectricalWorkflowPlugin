using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkCommands.Models
{
    public class HalfWireDesignation
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

        public override string ToString()
        {
            return Location + Appointment+ SparkProtection + Number + Suffix;
        }

        public NetTypes GetElectricalType()
        {
            return NetTypeClassificator.GetNetType(Appointment);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            var testObj = (HalfWireDesignation)obj;
            
            if(testObj.Location.Equals(Location) &&
               testObj.Appointment.Equals(Appointment) &&
               testObj.SparkProtection.Equals(SparkProtection) &&
               testObj.Number.Equals(Number) &&
               testObj.Suffix.Equals(Suffix)) 
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 0;

                // String properties
                hashCode = (hashCode * 397) ^ (Location  != null ? Location.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Appointment != null ? Location.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SparkProtection != null ? Appointment.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Number != null ? SparkProtection.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Suffix != null ? Number.GetHashCode() : 0);

                // int properties
                //hashCode = (hashCode * 397) ^ intProperty;
                return hashCode;
            }
        }
    }
}
