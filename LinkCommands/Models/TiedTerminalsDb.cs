using System.Collections.Generic;
using System.Linq;

namespace LinkCommands.Models
{
    public static class TiedTerminalsDb
    {
        private static Dictionary<IEnumerable<string>, IEnumerable<IEnumerable<string>>> _tiedTerminals = new()
        {
            { 
                // Component names
                new List<string>(){ "УЗЛ-СД-12", "УЗЛ-СД-24" },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "1","6" },
                    new() { "2","7" },
                    new() { "4","9" },
                    new() { "5","10"},
                }
            },
            { 
                // Component names
                new List<string>(){ "УЗП-12DC/5", "УЗП-24DC/5" },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "1","7" },
                    new() { "4","9" },
                    new() { "2","3" },
                }
            },
            { 
                // Component names
                new List<string>(){ "ГИС-К1/12", "ГИС-К1/24", "ГИС-К2/12", "ГИС-К2/24", "ГИС-К3/12", "ГИС-К3/24", },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "1","2" },
                    new() { "3","4" },
                }
            },
            { 
                // Component names
                new List<string>(){ "УЗЛ-И" },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "2","9" },
                    new() { "5","10"},
                    new() { "5","10"},
                    new() { "3","4","8" },
                }
            },
            { 
                // Component names
                new List<string>(){ "БИБ-02-24", "БИБ-02Р-24С" },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "1","5" },
                    new() { "2","3","6","7"},
                    new() { "4","8"},
                }
            },
            { 
                // Component names
                new List<string>(){ "DTNVR EXI", "DTNVR 1/24/1.5/1500" },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "1","2" },
                    new() { "3","4" },
                }
            },
            { 
                // Component names
                new List<string>(){ "DTNVR 2/24/1.5/1500" },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "1","2" },
                    new() { "3","4" },
                    new() { "5","6" },
                    new() { "7","8" },
                }
            },
            { 
                // Component names
                new List<string>(){ "РСТ 4П 24/90/0.5 Р" },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "1","2" },
                    new() { "3","4" },
                    new() { "5","6" },
                    new() { "7","8" },
                }
            },
            { 
                // Component names
                new List<string>(){ "БЗК", "БЗК ИСП.01", "БЗК ИСП.02" },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "+U","Вых.1" },
                    new() { "+U","Вых.2" },
                    new() { "+U","Вых.3" },
                    new() { "+U","Вых.4" },
                    new() { "+U","Вых.5" },
                    new() { "+U","Вых.6" },
                    new() { "+U","Вых.7" },
                    new() { "+U","Вых.8" },
                }
            },
        };

        public static IEnumerable<string> GetTiedTerminals(string componentName, string terminalDescription)
        {
            foreach (var item in _tiedTerminals)
            {
                if (item.Key.Contains(componentName))
                {
                    return GetTieds(item.Value, terminalDescription);
                }
            }
            return Enumerable.Empty<string>();
        }

        private static IEnumerable<string> GetTieds(IEnumerable<IEnumerable<string>> tieds, string description)
        {
            foreach (var tied in tieds)
            {
                if (tied.Contains(description))
                {
                    var restTieds = new List<string>();
                    foreach (var item in tied)
                    {
                        if (!item.Equals(description))
                        {
                            restTieds.Add(item);
                        }
                    }
                    return restTieds;
                }
            }
            return Enumerable.Empty<string>();
        }
    }
}
