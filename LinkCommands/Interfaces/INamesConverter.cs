using LinkCommands.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkCommands.Interfaces
{
    public interface INamesConverter
    {
        /// <summary>
        /// Get short name from two designation with automatic define source of full name.
        /// </summary>
        /// <param name="designation1">full designation 1</param>
        /// <param name="designation2">full designation 2</param>
        /// <returns>Short designation</returns>
        string GetShortName(HalfWireDesignation designation1, HalfWireDesignation designation2);
    }
}
