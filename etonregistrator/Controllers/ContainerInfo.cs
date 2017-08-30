using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace etonregistrator.Controllers
{
    public class ContainerInfo : IEqualityComparer<ContainerInfo>, IEqualityComparer<KeyValuePair<string, ContainerInfo>>
    {
        public string Id { get; set; }
        public string Host { get; set; }
        public ushort Port { get; set; }
        public string Name { get; set; }
        public ServiceAction Status { get; set; }

        public bool Equals(ContainerInfo x, ContainerInfo y)
        {
            return x.Id == y.Id;
        }

        public bool Equals(KeyValuePair<string, ContainerInfo> x, KeyValuePair<string, ContainerInfo> y)
        {
            return x.Key == y.Key;
        }

        public int GetHashCode(ContainerInfo obj)
        {
            return obj.Id.GetHashCode();
        }

        public int GetHashCode(KeyValuePair<string, ContainerInfo> obj)
        {
            return obj.Key.GetHashCode();
        }
    }
}
