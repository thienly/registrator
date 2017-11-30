using System;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using RegistratorWorker.Collector;
using Xunit;

namespace RegistratorWorkerTests
{
    public abstract class BaseContainerTest
    {
        protected DockerClientConfiguration _configuration =
            new DockerClientConfiguration(new Uri("tcp://10.0.19.107:2376"));
    }
    public class DockerContainersCollectorTest: BaseContainerTest
    {
        [Fact]
        public async Task Test_if_return_container_with_running_and_created_by_service()
        {
            //Arrange
            var collector = new DockerContainersCollector(_configuration);
            //Act
            var readOnlyCollection = await collector.GetAllRunningContainerThatIsCretedByService();
            //Assert
            Assert.NotEmpty(readOnlyCollection);
        }

        [Fact]
        public async Task Test_monitor()
        {
            //Arrange
            bool wasProgressCalled;
            var progress = new Progress<JSONMessage>((m) =>
            {
                var a = m;
                wasProgressCalled = true;
            });
            using (var client = _configuration.CreateClient())
            {
                var task = Task.Run(() => client.System.MonitorEventsAsync(new ContainerEventsParameters(), progress));
                await client.Containers.CreateContainerAsync(new CreateContainerParameters(new Config()
                {
                    Image = "registry.eton.vn/admin_service_uat",
                }));
                await task;
            }
        }
       
    }
}
