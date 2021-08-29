using System;
using System.Collections.Generic;

namespace AutocadTerminalsManager.Model
{
    /// <summary>
    /// Perimeter assembly of security and fire alarm systems.
    /// </summary>
    public interface IAssembly : ICloneable
    {
        /// <summary>
        /// Perimetric detector of the security and fire alarm system. 
        /// </summary>
        PerimeterDevice Device { get; set; }

        /// <summary>
        /// Perimeter cable for a fire alarm detector. 
        /// </summary>
        IEnumerable<Cable> PerimeterCable { get; set; }       
    }
}
