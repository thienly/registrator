using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RegistratorWorker.Common;

namespace RegistratorWorker.Manager
{
    public interface IManagerOperation
    {
        Task RegisterServices(IEnumerable<ContainerInfo> containerInfos);
        Task AddWorker(Uri workerUri);
        Task RemoveWorker(Uri uri);
        Task DeregisterServices(IEnumerable<ContainerInfo> containerInfos);
    }
}