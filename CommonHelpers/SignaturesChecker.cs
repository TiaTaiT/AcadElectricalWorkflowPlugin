using Autodesk.AutoCAD.DatabaseServices;

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
