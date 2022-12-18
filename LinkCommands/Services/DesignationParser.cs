using Autodesk.AutoCAD.DatabaseServices.Filters;
using CommonHelpers;
using LinkCommands.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LinkCommands.Services
{
    public static class DesignationParser
    {
        private static List<string> _powerSupplies = new()
        {
            "В", "GND", "V", "L", "N", "PE",
        };

        private static List<string> _signals = new()
        {
            "ШС", "КЦ",
        };

        private static List<string> _relays = new()
        {
            "NO", "NC", "COM", "K",
        };

        private static List<string> _remoteControl = new()
        {
            "ДК",
        };

        private static List<string> _rs485 = new()
        {
            "RS", "A", "B",
        };

        public static HalfWireDesignation GetDesignation(string designation)
        {
            var designationParts = StringUtils.GetStringNumbersWithPoint(designation);

            if(TryFindPowerSupply(designationParts, out var indexPower))
            {
                //GND, L, N, PE
                if (designationParts.ElementAt(indexPower).Equals("GND") ||
                    designationParts.ElementAt(indexPower).Equals("L") ||
                    designationParts.ElementAt(indexPower).Equals("N") ||
                    designationParts.ElementAt(indexPower).Equals("PE"))
                {
                    //GND
                    if (indexPower == 0 && designationParts.Count() == 1)
                    {
                        return new HalfWireDesignation()
                        {
                            Appointment = designationParts.ElementAt(indexPower)
                        };
                    }

                    //GND1, GND2, GND3...
                    if (indexPower == 0 && designationParts.Count() == 2)
                    {
                        return new HalfWireDesignation()
                        {
                            Appointment = designationParts.ElementAt(indexPower),
                            Number = designationParts.ElementAt(indexPower + 1),
                        };
                    }

                    //7.2GND, 7.2GND, 7.2GND...
                    if (indexPower == 1 && designationParts.Count() == 2)
                    {
                        return new HalfWireDesignation()
                        {
                            Location = designationParts.ElementAt(indexPower - 1),
                            Appointment = designationParts.ElementAt(indexPower),
                        };
                    }

                    //7.2GND1, 7.2GND2, 7.2GND3...
                    if (indexPower == 1 && designationParts.Count() == 3)
                    {
                        return new HalfWireDesignation()
                        {
                            Location = designationParts.ElementAt(indexPower - 1),
                            Appointment = designationParts.ElementAt(indexPower),
                            Number = designationParts.ElementAt(indexPower + 1),
                        };
                    }
                }

                //В || V
                if (designationParts.ElementAt(indexPower).Equals("В") || designationParts.ElementAt(indexPower).Equals("V"))
                {
                    //0В
                    if (indexPower == 1 && designationParts.Count() == 2)
                    {
                        return new HalfWireDesignation()
                        {
                            Appointment = designationParts.ElementAt(indexPower - 1) + designationParts.ElementAt(indexPower)
                        };
                    }

                    //0В1, 0В2, 0В3...
                    if (indexPower == 1 && designationParts.Count() == 3)
                    {
                        return new HalfWireDesignation()
                        {
                            Appointment = designationParts.ElementAt(indexPower - 1) + designationParts.ElementAt(indexPower),
                            Number = designationParts.ElementAt(indexPower + 1),
                        };
                    }

                    //+12В, +24В, +48В, +5V...
                    if (indexPower == 2 && designationParts.Count() == 3)
                    {
                        return new HalfWireDesignation()
                        {
                            Appointment = designationParts.ElementAt(indexPower - 2) +
                                          designationParts.ElementAt(indexPower - 1) +
                                          designationParts.ElementAt(indexPower),
                        };
                    }

                    //+12В1, +24В2, +48В3, +5V4, -5В5...
                    if (indexPower == 2 && designationParts.Count() == 4)
                    {
                        return new HalfWireDesignation()
                        {
                            Appointment = designationParts.ElementAt(indexPower - 2) +
                                          designationParts.ElementAt(indexPower - 1) +
                                          designationParts.ElementAt(indexPower),
                            Number = designationParts.ElementAt(indexPower + 1),
                        };
                    }
                }
            }

            if(TryFindShleif(designationParts, out var indexShleif))
            {
                //ШС4+, ШС5-, КЦ6-...
                if (indexShleif == 0 && designationParts.Count() == 3)
                {
                    return new HalfWireDesignation()
                    {
                        Appointment = designationParts.ElementAt(indexShleif),
                        Number = designationParts.ElementAt(indexShleif + 1),
                        Suffix = designationParts.ElementAt(indexShleif + 2),
                    };
                }

                //1.2ШС3+, 1.2ШС3-, 4КЦ5+...
                if (indexShleif == 1 && designationParts.Count() == 4)
                {
                    return new HalfWireDesignation()
                    {
                        Location = designationParts.ElementAt(indexShleif - 1),
                        Appointment = designationParts.ElementAt(indexShleif),
                        Number = designationParts.ElementAt(indexShleif + 1),
                        Suffix = designationParts.ElementAt(indexShleif + 2),
                    };
                }

                //ШСi4+, ШСi5-, КЦi6-...
                if (indexShleif == 0 && designationParts.Count() == 4)
                {
                    return new HalfWireDesignation()
                    {
                        Appointment = designationParts.ElementAt(indexShleif),
                        SparkProtection = designationParts.ElementAt(indexShleif + 1),
                        Number = designationParts.ElementAt(indexShleif + 2),
                        Suffix = designationParts.ElementAt(indexShleif + 3),
                    };
                }

                //1.2ШСi3+, 1.2ШСi3-, 4КЦi5+...
                if (indexShleif == 1 && designationParts.Count() == 5)
                {
                    return new HalfWireDesignation()
                    {
                        Location = designationParts.ElementAt(indexShleif - 1),
                        Appointment = designationParts.ElementAt(indexShleif),
                        SparkProtection = designationParts.ElementAt(indexShleif + 1),
                        Number = designationParts.ElementAt(indexShleif + 2),
                        Suffix = designationParts.ElementAt(indexShleif + 3),
                    };
                }
            }

            if(TryFindRelays(designationParts, out var indexRelay))
            {
                // NO, COM, NC, K
                if (indexRelay == 0 && designationParts.Count() == 1)
                {
                    return new HalfWireDesignation()
                    {
                        Appointment = designationParts.ElementAt(indexRelay),
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
                    };
                }

                // NO3(+24В), COM2(+12В),...
                if (indexRelay == 0 && designationParts.Count() == 5)
                {
                    return new HalfWireDesignation()
                    {
                        Location = designationParts.ElementAt(indexRelay - 1),
                        Appointment = designationParts.ElementAt(indexRelay),
                        Number = designationParts.ElementAt(indexRelay + 1),
                    };
                }

                // 1.6NO3(+12В), 4K(+12В)...
                if (indexRelay == 1 && designationParts.Count() == 6)
                {
                    return new HalfWireDesignation()
                    {
                        Location = designationParts.ElementAt(indexRelay - 1),
                        Appointment = designationParts.ElementAt(indexRelay),
                        Number = designationParts.ElementAt(indexRelay + 1),
                    };
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
                    
                    return new HalfWireDesignation()
                    {
                        Appointment = StringUtils.RemoveCharacters(withBrackets, '(', ')'),
                    };
                }

                //A, B
                if (indexRs == 0 && designationParts.Count() == 1)
                {
                    return new HalfWireDesignation()
                    {
                        Appointment = designationParts.ElementAt(indexRs),
                    };
                }

                //A1, B2
                if (indexRs == 0 && designationParts.Count() == 2)
                {
                    return new HalfWireDesignation()
                    {
                        Appointment = designationParts.ElementAt(indexRs),
                        Number = designationParts.ElementAt(indexRs + 1),
                    };
                }

                //3A1, 4B2
                if (indexRs == 1 && designationParts.Count() == 3)
                {
                    return new HalfWireDesignation()
                    {
                        Location = designationParts.ElementAt(indexRs- 1),
                        Appointment = designationParts.ElementAt(indexRs),
                        Number = designationParts.ElementAt(indexRs + 1),
                    };
                }
            }
            return new HalfWireDesignation();
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

        private static bool TryFindPowerSupply(IEnumerable<string> designationParts, out int index)
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
