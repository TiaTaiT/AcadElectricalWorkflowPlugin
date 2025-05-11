using AutocadTerminalsManager.Helpers;
using AutocadTerminalsManager.Model;

using Teigha.Runtime;

namespace AutocadTerminalsManager
{
    public class AcadCommand : IExtensionApplication
    {
        private const string _installApp = @"C:\Users\texvi\source\repos\Mega-lo-Man\TerminalsManagerUI\TerminalsManagerUI\bin\Debug\net6.0-windows\TerminalsManagerUI.exe";
        private const string _installArgs = "";
        private const string _jsonFilename = "assembly.json";

        public void Initialize()
        {

        }

        public void Terminate()
        {
        }

        [CommandMethod("GETTERMINALS", CommandFlags.Session)]
        public void GetTerminals()
        {
            string tempPath = System.IO.Path.GetTempPath();
            var _jsonPath = tempPath + _jsonFilename;

            var assemblyManager = new AssemblyManager();
            var assemblyList = assemblyManager.GetAssemblies(_jsonPath);

            foreach (var assembly in assemblyList)
            {
                Run(assembly);
            }
        }

        private static void Run(Assembly assembly)
        {
            var insertDrawing = new InsertDrawing(assembly.Device.BlockRef, assembly);
            var sourceIds = insertDrawing.GetSourceDrawingIds();
            insertDrawing.PutToTargetDb(sourceIds);
        }

        [CommandMethod("GETUITERMINALS", CommandFlags.Session)]
        public void GetUiTerminals()
        {
            var appResult = GetTerminalsHelper.StartTerminalsManager(_installApp, _installArgs);
            if (appResult != true) return;
            GetTerminals();
        }

        [CommandMethod("DRAWORDERDOWN", CommandFlags.Session)]
        public void MoveDown()
        {
            DrawOrderHelper.WipeoutsToBottom();
        }
    }
}
