using System.Collections.Generic;
using System.Threading.Tasks;
using RegistratorWorker.Common;

namespace RegistratorWorker.Collector
{
    public interface IDockerContainersCollector
    {
        Task<IReadOnlyCollection<ContainerInfo>> GetAllRunningContainerAndIsCretedByService();
    }
}