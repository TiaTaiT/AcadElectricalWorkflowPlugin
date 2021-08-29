using System.Collections.Generic;
using Newtonsoft.Json;

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

        
    }
}
