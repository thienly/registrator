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

        public async Task<IReadOnlyCollection<ContainerInfo>> GetAllRunningContainerAndIsCretedByService()
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
                        var name = x.Labels.FirstOrDefault(l => l.Key == "com.docker.swarm.service.name").Value;
                        var info = new ContainerInfo()
                        {
                            Id = x.ID,
                            Host = host,
                            Name = name,
                            Port = x.Ports.FirstOrDefault(f => f.PublicPort > 0).PublicPort
                        };
                        return info;
                    }).ToList();
                CollectorInternalData.Current.AddRange(serviceContainer);
                return serviceContainer;
            }
        }
    }
}
