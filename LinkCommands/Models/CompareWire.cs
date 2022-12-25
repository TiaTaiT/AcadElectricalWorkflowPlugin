using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace LinkCommands.Models
{
    public class CompareWire
    {
        private double _tolerance = 0.1;
        public double X { get; set; }
        public double Y { get; set; }
        public ObjectId WireId { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (GetType() != obj.GetType())
                return false;

            return (Math.Abs(this.X - ((CompareWire)obj).X) < _tolerance &&
                   Math.Abs(this.Y - ((CompareWire)obj).Y) < _tolerance);
        }

        public override int GetHashCode()
        {
            int hashCode = -2013743655;
            hashCode = hashCode * -1521134295 + _tolerance.GetHashCode();
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + WireId.GetHashCode();
            return hashCode;
        }
    }
}
