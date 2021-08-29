using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Internal;
using System;
using System.Collections.Generic;
using AutocadTerminalsManager.Helpers;
using AutocadTerminalsManager.Model;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutocadTerminalsManager
{
    public class AcadCommand : IExtensionApplication
    {
        private const string _installApp = @"C:\Users\texvi\source\repos\TerminalsManagerUI\TerminalsManagerUI\bin\Debug\net5.0-windows\TerminalsManagerUI.exe";
        private const string _installArgs = "";
        private const string _jsonPath = @"C:\Temp\assembly.json";

        public void Initialize()
        {   
        }

        public void Terminate()
        {
        }

        [CommandMethod("GetTerminals", CommandFlags.Session)]
        public void GetTerminals()
        {
            var appResult = GetTerminalsHelper.StartTerminalsManager(_installApp, _installArgs);
            if (appResult != true) return;
            var assemblyManager = new AssemblyManager();
            var assemblyList = assemblyManager.GetAssemblies(_jsonPath);

            foreach (var assembly in assemblyList)
            {
                var insertDrawing = new InsertDrawing(assembly.Device.BlockRef, assembly.PerimeterCables);
                var sourceIds = insertDrawing.GetSourceDrawingIds();
                insertDrawing.PutToTargetDb(sourceIds);
            }
        }
    }
}
