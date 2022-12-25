using CommonHelpers;
using LinkCommands.Interfaces;
using LinkCommands.Models;
using System.Collections.Generic;
using System.Linq;

namespace LinkCommands.Services
{
    public class DesignationParser : IDesignationParser
    {
        private const string _positiveChar = "+";
        private const string _negativeChar = "-";
        private const string _rs485aChar = "A";
        private const string _rs485bChar = "B";

        private static readonly List<string> _powerAc = new()
        {
            "L", "N", "PE",
        };

        private static readonly List<string> _powerSupplies = new()
        {
            "ПИ", "+U", "-U", ")В", "В", "GND", "V",
        };

        private static readonly List<string> _signals = new()
        {
            "ШС", "КЦ",
        };

        private static readonly List<string> _relays = new()
        {
            "NO", "NC", "COM", "K",
        };

        private static readonly List<string> _remoteControl = new()
        {
            "ДК",
        };

        private static readonly List<string> _rs485 = new()
        {
            "RS", "A", "B",
        };

        private static readonly List<string> _ladogaRs = new()
        {
            "ЛС+", "ЛС-"
        };

        public HalfWireDesignation GetDesignation(string designation)
        {
            var designationParts = StringUtils.GetStringNumbersWithPoint(designation);

            if (TryFindAcPowerSupply(designationParts, out var indexAcPower))
            {
                // L, N, PE,...
                if (designationParts.Count() == 1)
                {
                    return new HalfWireDesignation()
                    {
                        Appointment = designationParts.ElementAt(indexAcPower),
                        ElectricalType = NetTypes.PowerAc,
                    };
                }
                // L1, N2, PE3,...
                if (designationParts.Count() == 2)
                {
                    return new HalfWireDesignation()
                    {
                        Appointment = designationParts.ElementAt(indexAcPower),
                        Number = designationParts.ElementAt(indexAcPower + 1),
                        ElectricalType = NetTypes.PowerAc,
                    };
                }
            }

            if (TryFindDcPowerSupply(designationParts, out var indexDcPower))
            {
                //GND, L, N, PE
                if (designationParts.ElementAt(indexDcPower).Equals("GND"))
                {
                    //GND
                    if (indexDcPower == 0 && designationParts.Count() == 1)
                    {
                        return new HalfWireDesignation()
                        {
                            Appointment = designationParts.ElementAt(indexDcPower),
                            ElectricalType = NetTypes.PowerNegative,
                        };
                    }

                    //GND1, GND2, GND3...
                    if (indexDcPower == 0 && designationParts.Count() == 2)
                    {
                        return new HalfWireDesignation()
                        {
                            Appointment = designationParts.ElementAt(indexDcPower),
                            Number = designationParts.ElementAt(indexDcPower + 1),
                            ElectricalType = NetTypes.PowerNegative,
                        };
                    }

                    //7.2GND, 7.2GND, 7.2GND...
                    if (indexDcPower == 1 && designationParts.Count() == 2)
                    {
                        return new HalfWireDesignation()
                        {
                            Location = designationParts.ElementAt(indexDcPower - 1),
                            Appointment = designationParts.ElementAt(indexDcPower),
                            ElectricalType = NetTypes.PowerNegative,
                        };
                    }

                    //7.2GND1, 7.2GND2, 7.2GND3...
                    if (indexDcPower == 1 && designationParts.Count() == 3)
                    {
                        return new HalfWireDesignation()
                        {
                            Location = designationParts.ElementAt(indexDcPower - 1),
                            Appointment = designationParts.ElementAt(indexDcPower),
                            Number = designationParts.ElementAt(indexDcPower + 1),
                            ElectricalType = NetTypes.PowerNegative,
                        };
                    }
                }

                //В || V
                if (designationParts.ElementAt(indexDcPower).Equals("В") ||
                    designationParts.ElementAt(indexDcPower).Equals("V") ||
                    designationParts.ElementAt(indexDcPower).Equals("+U") ||
                    designationParts.ElementAt(indexDcPower).Equals("-U") ||
                    designationParts.ElementAt(indexDcPower).Equals("ПИ"))
                {
                    //0В
                    if (indexDcPower == 1 && designationParts.Count() == 2)
                    {
                        if (int.TryParse(designationParts.ElementAt(indexDcPower - 1), out var voltage))
                        {
                            return new HalfWireDesignation()
                            {
                                LowerVoltage = voltage,
                                UpperVoltage = voltage,
                                Appointment = designationParts.ElementAt(indexDcPower),
                                ElectricalType = NetTypes.PowerNegative,
                            };
                        }
                    }

                    //+U1, -U2,...
                    if (indexDcPower == 0 && designationParts.Count() == 2)
                    {
                        var sign = designationParts.ElementAt(indexDcPower).First().ToString();
                        return new HalfWireDesignation()
                        {
                            Appointment = designationParts.ElementAt(indexDcPower),
                            Number = designationParts.ElementAt(indexDcPower + 1),
                            ElectricalType = sign.Equals(_positiveChar) ? NetTypes.PowerPositive : NetTypes.PowerNegative,
                        };
                    }

                    // ПИ1+, ПИ2-,...
                    if (indexDcPower == 0 && designationParts.Count() == 3)
                    {
                        var suffix = designationParts.ElementAt(indexDcPower + 2);
                        return new HalfWireDesignation()
                        {
                            Appointment = designationParts.ElementAt(indexDcPower),
                            Number = designationParts.ElementAt(indexDcPower + 1),
                            Suffix = suffix,
                            ElectricalType = suffix.Equals(_positiveChar) ? NetTypes.PowerPositive : NetTypes.PowerNegative,
                        };
                    }

                    //0В1, 0В2, 0В3...
                    if (indexDcPower == 1 && designationParts.Count() == 3)
                    {
                        if (int.TryParse(designationParts.ElementAt(indexDcPower - 1), out var voltage))
                        {
                            return new HalfWireDesignation()
                            {
                                LowerVoltage = voltage,
                                UpperVoltage = voltage,
                                Appointment = designationParts.ElementAt(indexDcPower),
                                Number = designationParts.ElementAt(indexDcPower + 1),
                                ElectricalType = NetTypes.PowerNegative,
                            };
                        }
                    }

                    //+12В, +24В, +48В, +5V...
                    if (indexDcPower == 2 && designationParts.Count() == 3)
                    {
                        if (int.TryParse(designationParts.ElementAt(indexDcPower - 1), out var voltage))
                        {
                            return new HalfWireDesignation()
                            {
                                LowerVoltage = voltage,
                                UpperVoltage = voltage,
                                Appointment = designationParts.ElementAt(indexDcPower),
                                ElectricalType = NetTypes.PowerPositive,
                            };
                        }
                    }

                    //+12В1, +24В2, +48В3, +5V4, -5В5...
                    if (indexDcPower == 2 && designationParts.Count() == 4)
                    {
                        if (int.TryParse(designationParts.ElementAt(indexDcPower - 1), out var voltage))
                        {
                            return new HalfWireDesignation()
                            {
                                LowerVoltage = voltage,
                                UpperVoltage = voltage,
                                Appointment = designationParts.ElementAt(indexDcPower),
                                Number = designationParts.ElementAt(indexDcPower + 1),
                                ElectricalType = NetTypes.PowerPositive,
                            };
                        }
                    }
                }

                // )В
                if (designationParts.ElementAt(indexDcPower).Equals(")В") || designationParts.ElementAt(indexDcPower).Equals(")V"))
                {
                    // (10-28)В
                    if (indexDcPower == 4 && designationParts.Count() == 5)
                    {
                        var lowerVoltageStr = designationParts.ElementAt(1);
                        var upperVoltageStr = designationParts.ElementAt(3);
                        var voltageSign = designationParts.ElementAt(4);

                        if (int.TryParse(lowerVoltageStr, out var lowerVoltage) &&
                             int.TryParse(upperVoltageStr, out var upperVoltage) &&
                             voltageSign.Length == 2)
                        {
                            return new HalfWireDesignation()
                            {
                                LowerVoltage = lowerVoltage,
                                UpperVoltage = upperVoltage,
                                Appointment = voltageSign.Last().ToString(),
                                ElectricalType = NetTypes.PowerPositive,
                            };
                        }
                    }
                }

                // )В1
                if (designationParts.ElementAt(indexDcPower).Equals(")В") || designationParts.ElementAt(indexDcPower).Equals(")V"))
                {
                    // (10-28)В1
                    if (indexDcPower == 4 && designationParts.Count() == 6)
                    {
                        var lowerVoltageStr = designationParts.ElementAt(1);
                        var upperVoltageStr = designationParts.ElementAt(3);
                        var voltageSign = designationParts.ElementAt(4);

                        if (int.TryParse(lowerVoltageStr, out var lowerVoltage) &&
                             int.TryParse(upperVoltageStr, out var upperVoltage) &&
                             voltageSign.Length == 2)
                        {
                            return new HalfWireDesignation()
                            {
                                LowerVoltage = lowerVoltage,
                                UpperVoltage = upperVoltage,
                                Appointment = voltageSign.Last().ToString(),
                                Number = designationParts.ElementAt(5),
                                ElectricalType = NetTypes.PowerPositive,
                            };
                        }
                    }
                }
            }

            if (TryFindShleif(designationParts, out var indexShleif))
            {
                //ШС4+, ШС5-, КЦ6-...
                if (indexShleif == 0 && designationParts.Count() == 3)
                {
                    var suffix = designationParts.ElementAt(indexShleif + 2);
                    return new HalfWireDesignation()
                    {
                        Appointment = designationParts.ElementAt(indexShleif),
                        Number = designationParts.ElementAt(indexShleif + 1),
                        Suffix = suffix,
                        ElectricalType = suffix.Equals(_positiveChar) ? NetTypes.ShleifPositive : NetTypes.ShleifNegative,
                    };
                }

                //1.2ШС3+, 1.2ШС3-, 4КЦ5+...
                if (indexShleif == 1 && designationParts.Count() == 4)
                {
                    var suffix = designationParts.ElementAt(indexShleif + 2);
                    return new HalfWireDesignation()
                    {
                        Location = designationParts.ElementAt(indexShleif - 1),
                        Appointment = designationParts.ElementAt(indexShleif),
                        Number = designationParts.ElementAt(indexShleif + 1),
                        Suffix = suffix,
                        ElectricalType = suffix.Equals(_positiveChar) ? NetTypes.ShleifPositive : NetTypes.ShleifNegative,
                    };
                }

                //ШСi4+, ШСi5-, КЦi6-...
                if (indexShleif == 0 && designationParts.Count() == 4)
                {
                    var suffix = designationParts.ElementAt(indexShleif + 3);
                    return new HalfWireDesignation()
                    {
                        Appointment = designationParts.ElementAt(indexShleif),
                        SparkProtection = designationParts.ElementAt(indexShleif + 1),
                        Number = designationParts.ElementAt(indexShleif + 2),
                        Suffix = suffix,
                        ElectricalType = suffix.Equals(_positiveChar) ? NetTypes.ShleifPositive : NetTypes.ShleifNegative,
                    };
                }

                //1.2ШСi3+, 1.2ШСi3-, 4КЦi5+...
                if (indexShleif == 1 && designationParts.Count() == 5)
                {
                    var suffix = designationParts.ElementAt(indexShleif + 3);
                    return new HalfWireDesignation()
                    {
                        Location = designationParts.ElementAt(indexShleif - 1),
                        Appointment = designationParts.ElementAt(indexShleif),
                        SparkProtection = designationParts.ElementAt(indexShleif + 1),
                        Number = designationParts.ElementAt(indexShleif + 2),
                        Suffix = suffix,
                        ElectricalType = suffix.Equals(_positiveChar) ? NetTypes.ShleifPositive : NetTypes.ShleifNegative,
                    };
                }
            }

            if (TryFindRelays(designationParts, out var indexRelay))
            {
                // NO, COM, NC, K
                if (indexRelay == 0 && designationParts.Count() == 1)
                {
                    return new HalfWireDesignation()
                    {
                        Appointment = designationParts.ElementAt(indexRelay),
                        ElectricalType = NetTypes.Relay,
                    };
                }

                // NO1, COM2, NC3, K2
                if (indexRelay == 0 && designationParts.Count() == 2)
                {
                    return new HalfWireDesignation()
                    {
                        Appointment = designationParts.ElementAt(indexRelay),
                        Number = designationParts.ElementAt(indexRelay + 1),
                        ElectricalType = NetTypes.Relay,
                    };
                }

                // 1.6NO3, 6COM2,...
                if (indexRelay == 1 && designationParts.Count() == 3)
                {
                    return new HalfWireDesignation()
                    {
                        Location = designationParts.ElementAt(indexRelay - 1),
                        Appointment = designationParts.ElementAt(indexRelay),
                        Number = designationParts.ElementAt(indexRelay + 1),
                        ElectricalType = NetTypes.Relay,
                    };
                }

                // NO3(+24В), COM2(+12В),...
                if (indexRelay == 0 && designationParts.Count() == 5)
                {
                    if (int.TryParse(designationParts.ElementAt(indexRelay + 3), out var voltage))
                    {
                        return new HalfWireDesignation()
                        {
                            Appointment = designationParts.ElementAt(indexRelay),
                            Number = designationParts.ElementAt(indexRelay + 1),
                            ElectricalType = NetTypes.Relay,
                            UpperVoltage = voltage,
                            LowerVoltage = voltage,
                        };
                    }
                }

                // 1.6NO3(+12В), 4K(+12В)...
                if (indexRelay == 1 && designationParts.Count() == 6)
                {
                    if (int.TryParse(designationParts.ElementAt(indexRelay + 3), out var voltage))
                    {
                        return new HalfWireDesignation()
                        {
                            Location = designationParts.ElementAt(indexRelay - 1),
                            Appointment = designationParts.ElementAt(indexRelay),
                            Number = designationParts.ElementAt(indexRelay + 1),
                            ElectricalType = NetTypes.Relay,
                            UpperVoltage = voltage,
                            LowerVoltage = voltage,
                        };
                    }
                }
            }
            if (TryFindRemoteControl(designationParts, out var indexRemoteControl))
            {
                // ДК1+, ДК2+...
                if (indexRemoteControl == 0 && designationParts.Count() == 3)
                {
                    return new HalfWireDesignation()
                    {
                        Appointment = designationParts.ElementAt(indexRemoteControl),
                        Number = designationParts.ElementAt(indexRemoteControl + 1),
                        Suffix = designationParts.ElementAt(indexRemoteControl + 2),
                        ElectricalType = NetTypes.Relay,
                    };
                }

                // ДКi1+, ДКi2+...
                if (indexRemoteControl == 0 && designationParts.Count() == 4)
                {
                    return new HalfWireDesignation()
                    {
                        Appointment = designationParts.ElementAt(indexRemoteControl),
                        SparkProtection = designationParts.ElementAt(indexRemoteControl + 1),
                        Number = designationParts.ElementAt(indexRemoteControl + 2),
                        Suffix = designationParts.ElementAt(indexRemoteControl + 3),
                        ElectricalType = NetTypes.Relay,
                    };
                }

                // 1ДК1+, 3.4ДК2+...
                if (indexRemoteControl == 1 && designationParts.Count() == 4)
                {
                    return new HalfWireDesignation()
                    {
                        Location = designationParts.ElementAt(indexRemoteControl - 1),
                        Appointment = designationParts.ElementAt(indexRemoteControl),
                        Number = designationParts.ElementAt(indexRemoteControl + 1),
                        Suffix = designationParts.ElementAt(indexRemoteControl + 2),
                        ElectricalType = NetTypes.Relay,
                    };
                }

                // 1ДКi1+, 3.4ДКi2+...
                if (indexRemoteControl == 1 && designationParts.Count() == 5)
                {
                    return new HalfWireDesignation()
                    {
                        Location = designationParts.ElementAt(indexRemoteControl - 1),
                        Appointment = designationParts.ElementAt(indexRemoteControl),
                        SparkProtection = designationParts.ElementAt(indexRemoteControl + 1),
                        Number = designationParts.ElementAt(indexRemoteControl + 2),
                        Suffix = designationParts.ElementAt(indexRemoteControl + 3),
                        ElectricalType = NetTypes.Relay,
                    };
                }
            }

            if (TryFindRs485(designationParts, out var indexRs))
            {
                //RS485(A), RS485(B), RS485(GND)
                if (indexRs == 0 &&
                    designationParts.Count() == 3 &&
                    designationParts.ElementAt(indexRs + 1).Equals("485"))
                {
                    var withBrackets = designationParts.ElementAt(indexRs + 2);
                    var withoutBrackets = StringUtils.RemoveCharacters(withBrackets, '(', ')');

                    var electricalType = NetTypes.Rs485Gnd;
                    if (withoutBrackets.Equals(_rs485aChar))
                        electricalType = NetTypes.Rs485A;
                    if (withoutBrackets.Equals(_rs485bChar))
                        electricalType = NetTypes.Rs485B;

                    return new HalfWireDesignation()
                    {
                        Appointment = withoutBrackets,
                        ElectricalType = electricalType,
                    };
                }

                //A, B
                if (indexRs == 0 && designationParts.Count() == 1)
                {
                    var rsLetter = designationParts.ElementAt(indexRs);
                    return new HalfWireDesignation()
                    {
                        Appointment = rsLetter,
                        ElectricalType = rsLetter.Equals(_rs485aChar) ? NetTypes.Rs485A : NetTypes.Rs485B,
                    };
                }

                //A1, B2
                if (indexRs == 0 && designationParts.Count() == 2)
                {
                    var rsLetter = designationParts.ElementAt(indexRs);
                    return new HalfWireDesignation()
                    {
                        Appointment = rsLetter,
                        Number = designationParts.ElementAt(indexRs + 1),
                        ElectricalType = rsLetter.Equals(_rs485aChar) ? NetTypes.Rs485A : NetTypes.Rs485B,
                    };
                }

                //3A1, 4B2
                if (indexRs == 1 && designationParts.Count() == 3)
                {
                    var rsLetter = designationParts.ElementAt(indexRs);
                    return new HalfWireDesignation()
                    {
                        Location = designationParts.ElementAt(indexRs - 1),
                        Appointment = rsLetter,
                        Number = designationParts.ElementAt(indexRs + 1),
                        ElectricalType = rsLetter.Equals(_rs485aChar) ? NetTypes.Rs485A : NetTypes.Rs485B,
                    };
                }
            }
            if (TryFindLadogaRs(designationParts, out var indexLadogaRs))
            {
                //  ЛС+, ЛС-
                if (indexLadogaRs == 0 && designationParts.Count() == 1)
                {
                    var appointment = designationParts.ElementAt(indexLadogaRs)[0].ToString() +
                                      designationParts.ElementAt(indexLadogaRs)[1].ToString();
                    var suffix = designationParts.ElementAt(indexLadogaRs).Last().ToString();
                    return new HalfWireDesignation()
                    {
                        Appointment = appointment,
                        Suffix = suffix,
                        ElectricalType = suffix.Equals(_positiveChar) ? NetTypes.LadogaRsPositive : NetTypes.LadogaRsNegative,
                    };
                }
            }
            return new HalfWireDesignation();
        }

        private bool TryFindLadogaRs(IEnumerable<string> designationParts, out int indexLadogaRs)
        {
            return FindType(designationParts, _ladogaRs, out indexLadogaRs);
        }

        private bool TryFindAcPowerSupply(IEnumerable<string> designationParts, out int indexPower)
        {
            return FindType(designationParts, _powerAc, out indexPower);
        }

        private static bool TryFindRs485(IEnumerable<string> designationParts, out int index)
        {
            return FindType(designationParts, _rs485, out index);
        }

        private static bool TryFindRemoteControl(IEnumerable<string> designationParts, out int index)
        {
            return FindType(designationParts, _remoteControl, out index);
        }

        private static bool TryFindRelays(IEnumerable<string> designationParts, out int index)
        {
            return FindType(designationParts, _relays, out index);
        }

        private static bool TryFindShleif(IEnumerable<string> designationParts, out int index)
        {
            return FindType(designationParts, _signals, out index);
        }

        private static bool TryFindDcPowerSupply(IEnumerable<string> designationParts, out int index)
        {
            return FindType(designationParts, _powerSupplies, out index);
        }

        private static bool FindType(IEnumerable<string> designationParts,
                                     IEnumerable<string> types,
                                     out int index)
        {
            //if (designationParts.Intersect(types).Any())
            for (var i = 0; i < designationParts.Count(); i++)
            {
                if (types.Contains(designationParts.ElementAt(i)))
                {
                    index = i;
                    return true;
                }
            }
            index = 0;
            return false;
        }
    }
}
