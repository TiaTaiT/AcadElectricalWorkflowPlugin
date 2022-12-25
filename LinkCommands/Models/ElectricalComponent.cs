using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using CommonHelpers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LinkCommands.Models
{
    public class ElectricalComponent
    {
        public BlockReference BlockRef { get; set; }

        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public bool IsTerminal 
        {
            get => SignaturesChecker.IsTerminal(BlockRef);
            private set { } 
        }

        public List<ComponentTerminal> Terminals { get; set; } = new();

        public IEnumerable<Point3d> GetTerminalPoints()
        {
            var points = new List<Point3d>();
            foreach (var terminal in Terminals)
                points.AddRange(terminal.Points);
                    
            return points;
        }

        /// <summary>
        /// Get a list of all electrically tied terminals, including the desired terminal
        /// </summary>
        /// <param name="description">Desired terminal desscription</param>
        /// <returns>List of all electrically tied terminals</returns>
        public IEnumerable<ComponentTerminal> GetAllTiedTerminals(string description)
        {
            if(TryGetTerminalByDescription(description, out var foundTerminal))
                return foundTerminal.TiedTerminals;
            return Enumerable.Empty<ComponentTerminal>();
        }

        public ElectricalComponent()
        {

        }

        public ElectricalComponent(int id, 
                                   string name, 
                                   string designation, 
                                   List<ComponentTerminal> terminals,
                                   BlockReference blockReference)
        {
            Id = id;
            Name = name;
            Designation = designation;
            Terminals = terminals;
            BlockRef = blockReference;

            PopulateTiedTerminals();
            //DebugTiedTerminals();
        }

        private void DebugTiedTerminals()
        {
            Debug.WriteLine("=====================================================");
            Debug.WriteLine(Name + "  IsTerminal = " + IsTerminal.ToString());
            Debug.WriteLine("--------------------");
            foreach (var terminal in Terminals)
            {
                Debug.WriteLine(" + " + terminal.Value + "; Tieds: " + terminal.TiedTerminals.Count());
                foreach(var tied in terminal.TiedTerminals)
                {
                    Debug.WriteLine("   - " + tied.Value);
                }
                
            }
            Debug.WriteLine("=====================================================");
        }

        private void PopulateTiedTerminals()
        {
            foreach (var terminal in Terminals)
            {
                var tiedList = TiedTerminalsDb.GetTiedTerminals(Name, terminal.Value);

                foreach(var tied in tiedList)
                {
                    if (TryGetTerminalByDescription(tied, out var foundTerminal))
                    { 
                        terminal.TiedTerminals.Add(foundTerminal); 
                    }
                }
            }
        }

        private bool TryGetTerminalByDescription(string description, out ComponentTerminal foundTerminal)
        {
            foreach (var terminal in Terminals)
            {
                if (terminal.Value.ToUpper().Equals(description.ToUpper()))
                { 
                    foundTerminal = terminal; 
                    return true;
                }
            }
            foundTerminal = null;
            return false;
        }
    }
}
