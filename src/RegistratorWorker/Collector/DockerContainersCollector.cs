using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using RegistratorWorker.Common;

namespace RegistratorWorker.Collector
{
    public class DockerContainersCollector : IDockerContainersCollector
    {
        private readonly DockerClientConfiguration _dockerClientConfiguration;

        public DockerContainersCollector(DockerClientConfiguration dockerClientConfiguration)
        {
            _dockerClientConfiguration = dockerClientConfiguration;
        }

        public async Task<IReadOnlyCollection<ContainerInfo>> GetAllRunningContainerThatIsCretedByService()
        {
            using (var client = _dockerClientConfiguration.CreateClient())
            {
                var host = client.Configuration.EndpointBaseUri.Host;
                var filter = new Dictionary<string, IDictionary<string, bool>>();
                filter.Add("status", new Dictionary<string, bool>() {{"running", true}});
                var runningContainer = await client.Containers.ListContainersAsync(new ContainersListParameters()
                {
                    Filters = filter
                });
                var serviceContainer = runningContainer.Where(x =>
                    x.Labels.ContainsKey("com.docker.swarm.service.name") && x.Ports.Any(p => p.PublicPort > 0)).Select(
                    x =>
                    {
                        var name = x.Labels["com.docker.swarm.service.name"];
                        var id = x.Labels["com.docker.swarm.task.name"];
                        var info = new ContainerInfo()
                        {
                            Id = id,
                            Host = host,
                            Name = name,
                            Port = x.Ports.FirstOrDefault(f => f.PublicPort > 0).PublicPort
                        };
                        return info;
                    }).ToList();
                return serviceContainer;
            }
        }

        public async Task<IReadOnlyCollection<ContainerInfo>> GetAllRunningContainerThatIsNotRegister()
        {
            var readOnlyCollection = await GetAllRunningContainerThatIsCretedByService();
            var currentData = CollectorInternalData.Current.Data;
            var moreIds = readOnlyCollection.Select(x=>x.Id).ToList().Except(currentData.Select(x=>x.ID)).ToList();
            return readOnlyCollection.Where(x => moreIds.Contains(x.Id)).ToList();
        }
    }
}
