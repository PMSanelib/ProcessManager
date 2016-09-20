using System;
using System.IO;
using Newtonsoft.Json;

namespace ProcessManager
{
    public class ServiceConfigurationProvider
    {
        private const string ConfigFile = "ServiceDetails.json";
        private static readonly Lazy<ServiceConfiguration> Lazy = new Lazy<ServiceConfiguration>(() =>
        {
            var details = ReadConfigurations();

            return details;
        });

        public static ServiceConfiguration Instance => Lazy.Value;

        private ServiceConfigurationProvider()
        {
        }

        private static ServiceConfiguration ReadConfigurations()
        {
            if (!File.Exists(ConfigFile))
            {
                var details = new ServiceConfiguration();
                File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(details));
            }

            var str = File.ReadAllText(ConfigFile);
            return JsonConvert.DeserializeObject<ServiceConfiguration>(str);
        }

        public static void ConfigureSeedServices()
        {
            if (File.Exists(ConfigFile)) return;

            var config = Instance;

            config.NginxPath = "C:/Environment/nginx-1.11.3/nginx.exe";
            config.TempPath = "C:/temp";
            config.AddService("jayhawk.com", "C:/Projects/jayhawk/src/Web/bin/Debug/netcoreapp1.0/publish/Web.dll", 5001);
            config.AddService("libcloud.com", "C:/Projects/LibCloud/src/LibCloud/bin/Debug/netcoreapp1.0/publish/LibCloud.dll", 5002);

            UpdateConfiguration();
        }

        public static void UpdateConfiguration()
        {
            File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(Instance));
        }
    }
}