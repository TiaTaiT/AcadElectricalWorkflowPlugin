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
                new List<string>(){ "УЗЛ-СД-12", "УЗЛ-СД-24", },
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
                new List<string>(){ "УЗП-12DC/5", "УЗП-24DC/5", },
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
                new List<string>(){ "ГИС-К1/12", 
                                    "ГИС-К1/24", 
                                    "ГИС-К2/12", 
                                    "ГИС-К2/24", 
                                    "ГИС-К3/12", 
                                    "ГИС-К3/24", 
                                    "DTNVR EXI", 
                                    "DTNVR 1/24/1.5/1500", },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "1","2" },
                    new() { "3","4" },
                }
            },
            { 
                // Component names
                new List<string>(){ "УЗЛ-И", },
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
                new List<string>(){ "БИБ-02-24", "БИБ-02Р-24С", },
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
                new List<string>(){ "БИБ-04-24С", },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "1","9" },
                    new() { "2","3","10","11"},
                    new() { "4","12"},
                    
                    new() { "5","13" },
                    new() { "6","7","14","15"},
                    new() { "8","16"},
                }
            },
            { 
                // Component names
                new List<string>(){ "БИБ-08-24С", },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "1","17" },
                    new() { "2","3","18","19"},
                    new() { "4","20"},
                    
                    new() { "5","21" },
                    new() { "6","7","22","23"},
                    new() { "8","24"},

                    new() { "9","25" },
                    new() { "10","11","26","27"},
                    new() { "12","28"},

                    new() { "13","29" },
                    new() { "14","15","30","31"},
                    new() { "16","32"},
                }
            },
            { 
                // Component names
                new List<string>(){ "РИФ 24 EXI",  },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "1","3" },
                    new() { "2","4" },
                }
            },
            { 
                // Component names
                new List<string>(){ "DTNVR 2/24/1.5/1500", "DTNVR 2/24/0.8 F3G EXI" },
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
                new List<string>(){ "РСТ 4П 24/90/0.5 Р", },
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
                new List<string>(){ "БЗК", "БЗК ИСП.01", "БЗК ИСП.02", },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "(10-30)В1", "Вых.1" },
                    new() { "(10-30)В1", "Вых.2" },
                    new() { "(10-30)В1", "Вых.3" },
                    new() { "(10-30)В1", "Вых.4" },
                    new() { "(10-30)В1", "Вых.5" },
                    new() { "(10-30)В1", "Вых.6" },
                    new() { "(10-30)В1", "Вых.7" },
                    new() { "(10-30)В1", "Вых.8" },
                }
            },
            { 
                // Component names
                new List<string>(){ "С2000-БРИЗ", },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "1", "3", "6", "8" },
                    new() { "2", "4", "5", "7" },
                }
            },
            { 
                // Component names
                new List<string>(){ "БРШС-EX ИСП.2", },
                new List<List<string>>()
                {
                    // Tied terminals
                    new() { "3", "18" },
                    new() { "4", "17" },
                    new() { "5", "16" },
                    new() { "6", "15" },
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
