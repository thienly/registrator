using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RegistratorWorker.Collector;
using RegistratorWorker.Common;

namespace RegistratorWorker.Controllers
{
    [Route("api/worker/[controller]")]
    public class ContainerController : Controller
    {
        private readonly IDockerContainersCollector _dockerContainersCollector;
        public ContainerController(IDockerContainersCollector dockerContainersCollector)
        {
            _dockerContainersCollector = dockerContainersCollector;
        }

        [HttpGet]
        public async Task<IReadOnlyCollection<ContainerInfo>> GetAllRunningContainer()
        {
            return await _dockerContainersCollector.GetAllRunningContainerAndIsCretedByService();
        }
    }
}
