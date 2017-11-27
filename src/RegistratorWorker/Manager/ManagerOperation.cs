using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RegistratorWorker.Common;

namespace RegistratorWorker.Manager
{
    public class ManagerOperation : IManagerOperation
    {
        public Task RegisterServices(IEnumerable<ContainerInfo> containerInfos)
        {
            throw new NotImplementedException();
        }

        public Task AddWorker(Uri workerUri)
        {
            throw new NotImplementedException();
        }

        public Task RemoveWorker(Uri uri)
        {
            throw new NotImplementedException();
        }

        public Task DeregisterServices(IEnumerable<ContainerInfo> containerInfos)
        {
            throw new NotImplementedException();

        }
    }
}