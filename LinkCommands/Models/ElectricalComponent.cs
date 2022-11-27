using Autodesk.AutoCAD.DatabaseServices;
using LinkCommands.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkCommands.Models
{
    public class ElectricalComponent
    {
        public BlockReference BlockRef { get; set; }

        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public bool IsTerminal { get; set; } = false;

        public List<ComponentTerminal> Terminals { get; set; } = new();

        /// <summary>
        /// Get a list of all electrically tied terminals, including the desired terminal
        /// </summary>
        /// <param name="description">Desired terminal desscription</param>
        /// <returns>List of all electrically tied terminals</returns>
        public IEnumerable<ComponentTerminal> GetAllTiedTerminals(string description)
        {

            var terminal = GetTerminalByDescription(description);
            return terminal.TiedTerminals;
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

            if (IsOutputTerminal())
            {
                IsTerminal= true;
            }
            PopulateTiedTerminals();
            //DebugTiedTerminals();
        }

        private bool IsOutputTerminal()
        {
            return BlockRef.Name.StartsWith("VT0002_") || BlockRef.Name.StartsWith("HT0002_");
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
                    var tmp1 = GetTerminalByDescription(tied);
                    terminal.TiedTerminals.Add(tmp1);
                }
                
            }
        }

        private ComponentTerminal GetTerminalByDescription(string description)
        {
            foreach (var terminal in Terminals)
            {
                if(terminal.Value.ToUpper().Equals(description.ToUpper()))
                    return terminal;
            }
            throw new InvalidOperationException($"Terminal with description: {description} not found.");
        }
    }
}
