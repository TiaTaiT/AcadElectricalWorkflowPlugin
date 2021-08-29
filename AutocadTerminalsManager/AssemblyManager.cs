using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutocadTerminalsManager.Model;

namespace AutocadTerminalsManager
{
    public class AssemblyManager
    {

        public List<Assembly> GetAssemblies(string jsonPath)
        {
            var jsonString = File.ReadAllText(jsonPath);
            var assemblyList = JsonConvert.DeserializeObject<List<Assembly>>(jsonString);

            return assemblyList;
        }
    }
}
