using System.Diagnostics;

namespace AutocadTerminalsManager.Helpers
{
    public static class GetTerminalsHelper
    {
        private const int APP_JSON_GENERATE_SUCCESS = 100;

        public static bool StartTerminalsManager(string InstallApp, string InstallArgs)
        {
            var installProcess = new Process
            {
                StartInfo = { FileName = InstallApp, Arguments = InstallArgs }
            };
            //settings up parameters for the install process

            installProcess.Start();

            installProcess.WaitForExit();
            // Check for sucessful completion
            return installProcess.ExitCode == APP_JSON_GENERATE_SUCCESS ? true : false;
        }
    }
}
