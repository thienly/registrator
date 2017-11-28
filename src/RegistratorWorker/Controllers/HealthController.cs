using System;
using Microsoft.AspNetCore.Mvc;

namespace RegistratorWorker.Controllers
{
    [Route("api/[controller]")]
    public class HealthController : Controller
    {
        [Route("Status")]
        public IActionResult Status()
        {
            return Ok($"Registrator is okay {DateTime.Now}");
        }
    }
}
