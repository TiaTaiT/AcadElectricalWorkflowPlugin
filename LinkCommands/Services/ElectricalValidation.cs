using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (_rs485a.Contains(source) && _rs485a.Contains(destination))
                return true;

            if (_rs485b.Contains(source) && _rs485b.Contains(destination))
                return true;

            if (_rs485gnd.Contains(source) && _rs485gnd.Contains(destination))
                return true;

            return false;
        }

        private bool IsRs485(string source, string destination)
        {
            return _rs485a.Contains(source) ||
                   _rs485a.Contains(destination) ||
                   _rs485b.Contains(source) ||
                   _rs485b.Contains(destination) ||
                   _rs485gnd.Contains(source) ||
                   _rs485gnd.Contains(destination);
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

        private List<string> _rs485a = new List<string>(
            new string[] { "RS485A", "RS485(A)", "A", "A1", "A2", "A3", "A4" }
        );

        private List<string> _rs485b = new List<string>(
            new string[] { "RS485B", "RS485(B)", "B", "B1", "B2", "B3", "B4" }
        );

        private List<string> _rs485gnd = new List<string>(
            new string[] { "RS485GND", "RS485(GND)", "GND", "C", "C1", "C2", "C3", "C4" }
        );
    }
}