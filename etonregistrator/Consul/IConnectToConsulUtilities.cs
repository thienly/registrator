using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace etonregistrator.Consul
{
    public interface IConnectToConsulUtilities
    {
        Task ConnectToConsul(IEnumerable<ContainerInfo> containerInfos);
        Task DeregisterServiceThatAlreadyRegisterBy(IEnumerable<ContainerInfo> containerInfos);
    }
}
