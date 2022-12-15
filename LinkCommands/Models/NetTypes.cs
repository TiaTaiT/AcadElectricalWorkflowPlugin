using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkCommands.Models
{
    public enum NetTypes
    {
        Unknown,
        ShleifPositive,
        ShleifNegative,
        PowerNegative,
        PowerPositive,
        Rs485A,
        Rs485B,
        Rs485Gnd,
        DplsPositive,
        DplsNegative,
        Relay,
    }
}
