using System.Collections.Generic;
using System.IO;

namespace AutocadCommands.Services
{
    public class ConfigProvider
    {
        private string[] configLines;
        private const char AssignmentOperator = '=';
        private const char Separator = ';';

        private string DwgLibraryPath;

        public ConfigProvider(string filePath)
        {
            configLines = File.ReadAllLines(filePath);
            ConfigParse(configLines);
        }

        public IEnumerable<string> GreyTerminals { get; set; }
        public IEnumerable<string> YellowTerminals { get; set; }
        public IEnumerable<string> RedTerminals { get; set; }
        public IEnumerable<string> BlueTerminals { get; set; }
        public IEnumerable<string> WhiteTerminals { get; set; }
        public IEnumerable<string> OrangeTerminals { get; set; }
        public IEnumerable<string> BlackTerminals { get; set; }
        public IEnumerable<string> GreenYellowTerminals { get; set; }

        private void ConfigParse(IEnumerable<string> cfgLines)
        {
            foreach (var line in cfgLines)
            {
                var operands = line.Split(AssignmentOperator);
                if (operands.Length == 2 && operands[0].Length > 0)
                {
                    switch (operands[0].ToUpper())
                    {
                        case "DWGLIBPATH":
                            DwgLibraryPath = operands[1];
                            break;

                        case "GREEN-YELLOW":
                            GreenYellowTerminals = operands[1].Split(Separator);
                            break;

                        case "BLACK":
                            BlackTerminals = operands[1].Split(Separator);
                            break;

                        case "ORANGE":
                            OrangeTerminals = operands[1].Split(Separator);
                            break;

                        case "WHITE":
                            WhiteTerminals = operands[1].Split(Separator);
                            break;

                        case "BLUE":
                            BlueTerminals = operands[1].Split(Separator);
                            break;

                        case "RED":
                            RedTerminals = operands[1].Split(Separator);
                            break;

                        case "YELLOW":
                            YellowTerminals = operands[1].Split(Separator);
                            break;

                        case "GREY":
                            GreyTerminals = operands[1].Split(Separator);
                            break;
                    }
                }
            }
        }
    }
}