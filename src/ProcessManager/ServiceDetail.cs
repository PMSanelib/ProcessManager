using Newtonsoft.Json;

namespace ProcessManager
{
    public class ServiceDetail
    {
        [JsonIgnore]
        public int ProcessId { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public int Port { get; set; }
    }
}