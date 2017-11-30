using Microsoft.AspNetCore.Mvc;
using RegistratorWorker.Common;

namespace RegistratorWorker.Controllers
{
    [Route("api/[controller]")]
    public class ManagerOperationController : Controller
    {
        public ManagerOperationController()
        {
        }

        [HttpGet]
        [Route("AddWorker")]
        public IActionResult AddWorker(ServerInfo serverInfo)
        {
            return Ok();
        }
        
    }
}