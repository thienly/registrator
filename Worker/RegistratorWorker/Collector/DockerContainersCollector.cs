using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using etonregistrator.Consul;

namespace RegistratorWorker.Collector
{
    public class DockerContainersCollector : IDockerContainersCollector
    {
        public Task<IReadOnlyCollection<ContainerInfo>> Query()
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyCollection<ContainerInfo>> Query(string pattern)
        {
            throw new System.NotImplementedException();
        }
    }
}
