using System;
using System.Collections.Generic;
using System.Linq;

namespace AutocadTerminalsManager.Model
{
    public class PerimeterDevice : BaseComponent
    {
        /// <summary>
        /// Perimeter detector brand
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Description of the purpose of the perimeter detector
        /// </summary>
        public string DeviceDescription { get; set; }

        /// <summary>
        /// Perimeter detector terminal list 
        /// </summary>
        public string TerminalString { get; set; } // For store in DB
        public List<string> TerminalsList
        {
            get => TerminalString.Split(';').ToList();
            set => TerminalString = String.Join(";", value);
        }

        /// <summary>
        /// Autocad block reference file path
        /// </summary>
        public string BlockRef { get; set; }

        /// <summary>
        /// Schematic diagram of this detector PNG, JPG
        /// </summary>
        public string ImagePath { get; set; }

        public PerimeterDevice()
        {
            TerminalsList = new List<string>();
        }

        public object Clone()
        {
            var newPerimeterDevice = new PerimeterDevice()
            {
                Id = Id,
                ErpCode = ErpCode,
                DeviceName = DeviceName,
                DeviceDescription = DeviceDescription,
                TerminalString = TerminalString,
                BlockRef = BlockRef,
                ImagePath = ImagePath
            };
            return newPerimeterDevice;
        }

        public override bool Equals(object obj)
        {
            return obj is PerimeterDevice device &&
                   Id == device.Id &&
                   ErpCode == device.ErpCode &&
                   DeviceName == device.DeviceName &&
                   DeviceDescription == device.DeviceDescription &&
                   TerminalString == device.TerminalString &&
                   //EqualityComparer<List<string>>.Default.Equals(TerminalsList, device.TerminalsList) &&
                   BlockRef == device.BlockRef &&
                   ImagePath == device.ImagePath;
        }

        public override int GetHashCode()
        {
            int hashCode = 878515461;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ErpCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DeviceName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DeviceDescription);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TerminalString);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(BlockRef);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ImagePath);
            return hashCode;
        }
    }
}
