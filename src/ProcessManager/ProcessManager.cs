using System.Diagnostics;
using System.IO;
using NLog;

namespace ProcessManager
{
    public class ProcessManager
    {
        public static void RunNginx()
        {
            var logger = LogManager.GetLogger("Manager");
            logger.Info("Checking nginx service");

            var isRunning = false;
            foreach (var process in Process.GetProcessesByName("nginx"))
            {
                logger.Info($"Process nginx found with PID: {process.Id}");
                isRunning = true;
            }

            if (isRunning) return;

            logger.Info("Process nginx not running. Attempting to start");

            var nginxPath = ServiceConfigurationProvider.Instance.NginxPath;
            var workingDirectory = Path.GetDirectoryName(nginxPath) ?? Path.GetTempPath();

            var p = new Process
            {
                StartInfo =
                     {
                         FileName = nginxPath,
                         WorkingDirectory = workingDirectory,
                         CreateNoWindow = true,
                         UseShellExecute = false,
                         RedirectStandardError = false,
                         RedirectStandardOutput = false,
                         RedirectStandardInput = false
                     }
            };

            p.Start();

            logger.Info($"Process start event triggered. PID:{p.Id}");
        }

        public static void ReloadNginx()
        {
            var logger = LogManager.GetLogger("Manager");
            logger.Info("Reloading nginx service");

            var nginxPath = ServiceConfigurationProvider.Instance.NginxPath;
            var nginxDirectory = Path.GetDirectoryName(nginxPath) ?? Path.GetTempPath();

            var kestrelDirectory = nginxDirectory + "\\kestrel-sites";

            var kestrelDirectoryInfo = new DirectoryInfo(kestrelDirectory);

            foreach (var file in kestrelDirectoryInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (var serviceDetail in ServiceConfigurationProvider.Instance.Services)
            {
                var content = File.ReadAllText("nginx-site-template.conf");
                File.WriteAllText($"{kestrelDirectory}\\{serviceDetail.Name}.conf", content.Replace("$name$", serviceDetail.Name).Replace("$port$", serviceDetail.Port.ToString()));
            }

            var p = new Process
            {
                StartInfo =
                     {
                         FileName = nginxPath,
                         Arguments = "-s reload",
                         WorkingDirectory = nginxDirectory,
                         CreateNoWindow = true,
                         UseShellExecute = false,
                         RedirectStandardError = false,
                         RedirectStandardOutput = false,
                         RedirectStandardInput = false
                     }
            };

            p.Start();

            logger.Info($"Process nginx reloaded. PID:{p.Id}");
        }

        public static void StartServices()
        {
            var configuration = ServiceConfigurationProvider.Instance;

            var runningProcesses = Process.GetProcessesByName("dotnet");

            foreach (var detail in configuration.Services)
            {
                LogHelper.AddLogger(detail.Name);

                var logger = LogManager.GetLogger(detail.Name);

                var fileName = Path.GetFileName(detail.FilePath);
                var workingDirectory = Path.GetDirectoryName(detail.FilePath);

                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(workingDirectory))
                {
                    logger.Error($"{fileName} in {workingDirectory} not found. Skipping this service.");
                    continue;
                }

                foreach (var p in runningProcesses)
                {
                    var commandLine = p.GetCommandLine();
                    if (!commandLine.Contains($"{fileName} --server.urls=http://*:{detail.Port}")) continue;
                    detail.ProcessId = p.Id;
                    break;
                }

                if (detail.ProcessId > 0)
                {
                    logger.Info($"Service {detail.Name} is already runnig with PID: {detail.ProcessId}");
                    continue;
                }

                var process = new Process
                {
                    StartInfo =
                       {
                           FileName = "dotnet",
                           Arguments = $"exec {fileName} --server.urls=http://*:{detail.Port}",
                           WorkingDirectory = workingDirectory,
                           UseShellExecute = false,
                           CreateNoWindow = true,
                           RedirectStandardError = true,
                           RedirectStandardOutput = true,
                           RedirectStandardInput = true
                       }
                };

                process.OutputDataReceived += (s, e) =>
                {
                    logger.Info(e.Data);
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    logger.Error(e.Data);
                };

                process.Start();

                //* Read one element asynchronously
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                detail.ProcessId = process.Id;

                logger.Info($"Service {detail.Name} started with PID: {detail.ProcessId}");
                
            }
        }
    }
}