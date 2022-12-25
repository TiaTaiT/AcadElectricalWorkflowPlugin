using LinkCommands.Models;

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
