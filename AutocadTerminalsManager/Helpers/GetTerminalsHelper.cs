using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using AutocadTerminalsManager.Services;
using Autodesk.AutoCAD.Windows.Data;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AutocadTerminalsManager.Helpers
{
    public static class GetTerminalsHelper
    {
        private const int APP_JSON_GENERATE_SUCCESS = 100;
        
        public static bool StartTerminalsManager(string InstallApp, string InstallArgs)
        {
            var installProcess = new Process
            {
                StartInfo = {FileName = InstallApp, Arguments = InstallArgs}
            };
            //settings up parameters for the install process

            installProcess.Start();

            installProcess.WaitForExit();
            // Check for sucessful completion
            return installProcess.ExitCode == APP_JSON_GENERATE_SUCCESS ? true : false;
        }
    }
}
