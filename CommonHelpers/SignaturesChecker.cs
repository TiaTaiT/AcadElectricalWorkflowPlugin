using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers
{
    public static class SignaturesChecker
    {
        public static bool IsTerminal(BlockReference blockRef)
        {
            return blockRef.Name.StartsWith("VT0002_") || blockRef.Name.StartsWith("HT0002_");
        }
    }
}
