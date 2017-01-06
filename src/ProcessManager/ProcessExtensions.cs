using System.Diagnostics;
using System.IO;

namespace ProcessManager
{
    public static class ProcessExtensions
    {
        internal static string GetCommandLine(this Process process)
        {
            bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

            if (!isWindows)
            {
                throw new System.NotImplementedException("Only windows implementations");
            }

            var output = RunCommand($"PROCESS WHERE Processid={process.Id} get Commandline");

            var commandLine = "";

            using (var strReader = new StringReader(output))
            {
                var pathAvailable = false;
                do
                {
                    var line = strReader.ReadLine();
                    if(line == "CommandLine")
                    {
                        pathAvailable = true;
                    }
                    if(pathAvailable)
                    {
                        commandLine = line;
                    }

                } while (strReader.Peek() != -1);
            }

            return commandLine.ToString();
        }

        private static string RunCommand(string args)
        {
            var process = new Process();

            process.StartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                FileName = "wmic",
                CreateNoWindow = true
            };

            process.StartInfo.Arguments = args;
            process.Start();
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return output;
        }
    }
}