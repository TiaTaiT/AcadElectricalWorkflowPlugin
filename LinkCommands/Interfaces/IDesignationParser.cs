using LinkCommands.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkCommands.Interfaces
{
    public interface IDesignationParser
    {
        /// <summary>
        /// Get object describes designation of the "HalfWire"
        /// </summary>
        /// <param name="designation">Raw designation string</param>
        /// <returns>Half wire designation object</returns>
        HalfWireDesignation GetDesignation(string designation);
    }
}
