using System.Collections.Generic;
using System.Threading.Tasks;
using EtonRegistratorShared;

namespace RegistratorWorker.Collector
{
    public interface IDockerContainersCollector
    {
        Task<IReadOnlyCollection<ContainerInfo>> GetAllRunningContainerAndIsCretedByService();
    }
}