using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Xml.Linq;

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
            if(source == null || destination == null)
                IsValid = false;

            if(IsRs485(source, destination))
            {
                if (!CheckValidRs485(source, destination))
                {
                    ErrorMessage = "Signal type was define as RS485, but link is not correct";
                    IsValid = false;
                }
                ShortName = WireNameGenerator.GetShortWireName(source, destination, WireNameGenerator.SignalType.Rs485);
                
                return;
            }

            if(IsShleif(source, destination))
            {
                if (!CheckValidShleif(source, destination))
                {
                    ErrorMessage = "Signal type was define as shleif, but link is not correct";
                    IsValid = false;
                }
                ShortName = WireNameGenerator.GetShortWireName(source, destination, WireNameGenerator.SignalType.Shleif);
                
                return;
            }

            ShortName = GetApproximateName(source, destination);
            ErrorMessage = "Unrecognized signal type!";
            IsValid = false;
        }

        private bool CheckValidShleif(string source, string destination)
        {
            return source.Contains("ШС") ||
               destination.Contains("ШС") ||
               source.Contains("КЦ") ||
               destination.Contains("КЦ") ||
               IsNumeric(source) ||
               IsNumeric(destination);
        }

        private bool IsShleif(string source, string destination)
        {
            return source.Contains("ШС") ||
               destination.Contains("ШС") ||
               source.Contains("КЦ") ||
               destination.Contains("КЦ");
        }

        private string GetApproximateName(string source, string destination)
        {
            if (IsNumeric(source))
            {
                return destination; 
            }
            if (IsNumeric(destination))
            { 
                return source; 
            }
            return source;
        }

        private bool CheckValidRs485(string source, string destination)
        {
            if(source.Contains("A") && destination.Contains("A"))
                return true;
            if (source.Contains("B") && destination.Contains("B"))
                return true;
            return false;
        }

        private bool IsRs485(string source, string destination)
        {
            return source.StartsWith("RS485") || 
               destination.StartsWith("RS485") || 
               source.StartsWith("A") || 
               destination.StartsWith("A") ||
               source.StartsWith("B") ||
               destination.StartsWith("B");
        }

        private bool IsNumeric(string s)
        {
            foreach (char c in s)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }
    }
}