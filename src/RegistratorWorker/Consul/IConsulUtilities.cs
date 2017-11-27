using System.Collections.Generic;
using System.Threading.Tasks;
using Consul;
using RegistratorWorker.Common;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RegistratorWorker.Consul
{
    public interface IConsulUtilities
    {
        Task PushData(IEnumerable<ContainerInfo> containerInfos);
        Task DeregisterService(IEnumerable<ContainerInfo> containerInfos);
        ConsulClient CreateConsulClient();
        ConsulClient CreateConsulClient(string consulAddress);
    }
}
