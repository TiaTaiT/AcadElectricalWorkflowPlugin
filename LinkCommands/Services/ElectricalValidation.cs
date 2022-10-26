using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using CommonHelpers;
using LinkCommands.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static Autodesk.AutoCAD.DatabaseServices.RenderGlobal;
using static System.Net.Mime.MediaTypeNames;

namespace LinkCommands.Services
{
    internal class ElectricalValidation
    {
        public string ErrorMessage { get; set; } = "";

        public bool IsValid { get; set; } = true;

        public string ShortName { get; set; } = "";

        public ElectricalValidation(string sourceDescription, string destinationDescription)
        {
            ValidateWire(sourceDescription, destinationDescription);
        }
        public void ValidateWire(string source, string destination)
        {
            if (source == null || destination == null)
            {
                IsValid = false;
                return;
            }
            if (IsRs485(source, destination))
            {
                if (!CheckValidRs485(source, destination))
                {
                    ErrorMessage = "Signal type was define as RS485, but link is not correct";
                    IsValid = false;
                    //ShortName = GetApproximateName(source, destination);

                    //return;
                }
                ShortName = WireNameGenerator.GetShortWireName(source, destination, WireNameGenerator.SignalType.Rs485);

                return;
            }

            if (IsShleif(source, destination))
            {
                if (!CheckValidShleif(source, destination))
                {
                    ErrorMessage = "Signal type was define as shleif, but link is not correct";
                    IsValid = false;
                }
                ShortName = WireNameGenerator.GetShortWireName(source, destination, WireNameGenerator.SignalType.Shleif);

                return;
            }

            if (IsKc(source, destination))
            {
                if (!CheckValidKc(source, destination))
                {
                    ErrorMessage = "Signal type was define as shleif, but link is not correct";
                    IsValid = false;
                }
                ShortName = WireNameGenerator.GetShortWireName(source, destination, WireNameGenerator.SignalType.KC);

                return;
            }

            if (IsPower(source, destination))
            {
                if (!CheckValidPower(source, destination))
                {
                    ErrorMessage = "Signal type was define as shleif, but link is not correct";
                    IsValid = false;
                }
                ShortName = WireNameGenerator.GetShortWireName(source, destination, WireNameGenerator.SignalType.Power);

                return;
            }

            ShortName = GetApproximateName(source, destination);
            ErrorMessage = "Unrecognized signal type!";
            IsValid = false;
        }

        private bool CheckValidKc(string source, string destination)
        {
            return true;
        }

        private bool CheckValidShleif(string source, string destination)
        {
            return true;
        }

        private bool CheckValidPower(string source, string destination)
        {
            if ((source.StartsWith("+") && (destination.StartsWith("-") || destination.StartsWith("0В") || destination.Contains("GND"))) ||
                ((source.StartsWith("-") || source.StartsWith("0В") || source.Contains("GND")) && destination.StartsWith("+")))
                return false;

            var sourceVoltageRange = GetVoltageRange(source); // диапазон напряжений (минимум, максимум)
            var destinationVoltageRange = GetVoltageRange(destination);

            return Mathematic.AreRangesIntersect(sourceVoltageRange, destinationVoltageRange);
        }

        private (int, int) GetVoltageRange(string str)
        {
            if (str.StartsWith("+") || str.StartsWith("-") || str.StartsWith("~"))
            {
                var voltageStr = str.Substring(1);
                var result = int.TryParse(GetFirstDigits(voltageStr), out var output);

                return (output, output);
            }

            if (str.StartsWith("(") && str.Contains("-") && str.EndsWith(")В"))
            {
                var numbers = Mathematic.GetAllNumbers(str);

                if (numbers.Count != 2)
                    throw new System.Exception("Numbers count != 2");

                return (numbers.First(), numbers.Last());
            }

            return (0, 0);
        }

        private bool IsShleif(string source, string destination)
        {
            return source.Contains("ШС") ||
               destination.Contains("ШС");
        }

        private bool IsKc(string source, string destination)
        {
            return source.Contains("КЦ") ||
               destination.Contains("КЦ");
        }
        private bool IsPower(string source, string destination)
        {
            return source.StartsWith("+") ||
               destination.StartsWith("+") ||
               source.StartsWith("-") ||
               destination.StartsWith("-") ||
               source.StartsWith("~") ||
               destination.StartsWith("~") ||
               source.StartsWith("0В") ||
               destination.StartsWith("0В") ||
               source.StartsWith("GND") ||
               destination.StartsWith("GND") ||
               (source.StartsWith("(") && source.EndsWith(")В")) ||
               (destination.StartsWith("(") && destination.EndsWith(")В"));
        }

        private string GetApproximateName(string source, string destination)
        {
            if (Mathematic.IsNumeric(source))
            {
                return destination;
            }
            if (Mathematic.IsNumeric(destination))
            {
                return source;
            }
            return source;
        }

        private bool CheckValidRs485(string source, string destination)
        {
            var sourceType = NetTypeClassificator.GetNetTypes(source);
            var destinationType = NetTypeClassificator.GetNetTypes(destination);

            if (sourceType == destinationType)
                return true;
            
            return false;
        }

        private bool IsRs485(string source, string destination)
        {
            return NetTypeClassificator.IsRs485A(source) ||
                   NetTypeClassificator.IsRs485A(destination) ||
                   NetTypeClassificator.IsRs485B(source) ||
                   NetTypeClassificator.IsRs485B(destination) ||
                   NetTypeClassificator.IsRs485Gnd(source) ||
                   NetTypeClassificator.IsRs485Gnd(destination);
        }

        

        private string GetFirstDigits(string inputStr)
        {
            var firstDigitsCharacters = inputStr.TakeWhile(c => char.IsDigit(c));
            return new string(firstDigitsCharacters.ToArray());
        }

        
    }
}