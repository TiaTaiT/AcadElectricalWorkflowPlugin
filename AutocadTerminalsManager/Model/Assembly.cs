using Newtonsoft.Json;
using System.Collections.Generic;

namespace AutocadTerminalsManager.Model
{
    public class Assembly
    {
        [JsonConstructor]
        public Assembly()
        {
        }

        public Assembly(PerimeterDevice device)
        {
            Device = device;
        }

        public Assembly(PerimeterDevice device, IEnumerable<Cable> perimeterCable)
        {
            Device = device;
            PerimeterCables = perimeterCable;
        }

        public PerimeterDevice Device { get; set; }
        public IEnumerable<Cable> PerimeterCables { get; set; }
        public bool IsSourgeProtection { get; set; }
        public bool IsExplosionProof { get; set; }

    }
}
