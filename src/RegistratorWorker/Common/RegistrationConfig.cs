using System.Collections.Generic;
using Newtonsoft.Json;

namespace RegistratorWorker.Common
{
    public class RegistrationConfig
    {
        
        public string Host { get; set; }
        public string Name { get; set; }
        public string DockerHost { get; set; }
        public List<string> DockerHostArray => JsonConvert.DeserializeObject<List<string>>(DockerHost);
        public string Role { get; set; }
        public string Consul { get; set; }
        public int IntervalCheck { get; set; }
        public int DeregisterServiceAfter { get; set; }
        public int TimeOut { get; set; }
    }
}
