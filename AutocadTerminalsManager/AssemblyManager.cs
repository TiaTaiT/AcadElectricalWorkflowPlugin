using AutocadTerminalsManager.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

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
