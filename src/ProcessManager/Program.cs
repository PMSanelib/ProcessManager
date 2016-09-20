using System;
using System.Diagnostics;

namespace ProcessManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceConfigurationProvider.ConfigureSeedServices();

            LogHelper.AddLogger("Manager");

            ProcessManager.RunNginx();
            ProcessManager.StartServices();
            ProcessManager.ReloadNginx();

            string line;
            while ((line = Console.ReadLine()) != "exit")
            {
                int pid;
                if (int.TryParse(line, out pid))
                {
                    Process.GetProcessById(pid).Kill();
                    continue;
                }

                foreach (var process in Process.GetProcessesByName(line))
                {
                    Console.WriteLine(process.Id + " : " + process.GetCommandLine());
                }
            }
        }
    }
}
