using System.Collections.Generic;
using System.Threading.Tasks;
using etonregistrator.Consul;

namespace RegistratorWorker.Collector
{
    public interface IDockerContainersCollector
    {
        Task<IReadOnlyCollection<ContainerInfo>> Query();
        Task<IReadOnlyCollection<ContainerInfo>> Query(string pattern);
    }
}