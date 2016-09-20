using System.Collections.Generic;

namespace ProcessManager
{
    public class ServiceConfiguration
    {
        public string NginxPath { get; set; }

        public string TempPath { get; set; }
        public List<ServiceDetail> Services { get; set; }

        public ServiceConfiguration()
        {
            Services = new List<ServiceDetail>();
        }

        public void AddService(string name, string filePath, int port)
        {
            Services.Add(new ServiceDetail
            {
                Name = name,
                Port = port,
                FilePath = filePath
            });
        }
    }
}

/*
 *  private static readonly Lazy<IConfigurationRoot> Lazy = new Lazy<IConfigurationRoot>(() =>
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            return builder.Build();
        });

        public static IConfigurationRoot Instance => Lazy.Value;
 */
