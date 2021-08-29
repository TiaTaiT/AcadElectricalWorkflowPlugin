using System.Collections.Generic;

namespace AutocadTerminalsManager.Model
{
    public class Cable : BaseComponent
    {
        /// <summary>
        /// Cable brand
        /// </summary>
        public string Brand { get; set; }

        /// <summary>
        /// Number of wires in the cable
        /// </summary>
        public int WiresNumber { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool IsArmoured { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Cable cable &&
                   Id == cable.Id &&
                   Brand == cable.Brand &&
                   WiresNumber == cable.WiresNumber &&
                   IsArmoured == cable.IsArmoured;
        }

        public override int GetHashCode()
        {
            int hashCode = 1501468642;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Brand);
            hashCode = hashCode * -1521134295 + WiresNumber.GetHashCode();
            hashCode = hashCode * -1521134295 + IsArmoured.GetHashCode();
            return hashCode;
        }
    }
}
