using System;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Xunit;

namespace RegistratorWorkerTests
{
    public class DockerServiceQueryTests
    {
        [Fact]
        public async Task Test()
        {
            var configuration = new DockerClientConfiguration(new Uri("tcp://10.0.19.107:2376"));
            using (var client = configuration.CreateClient())
            {
                var swarmServices = await client.Swarm.ListServicesAsync();
                var services = swarmServices.ToList();
                Assert.NotEmpty(services);
            }
        }
    }
}
