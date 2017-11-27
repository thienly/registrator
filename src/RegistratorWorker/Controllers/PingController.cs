using Microsoft.AspNetCore.Mvc;

namespace RegistratorWorker.Controllers
{
    [Route("api/[controller]")]
    public class PingController : Controller
    {
        // GET api/values
        [HttpGet]
        public string Get()
        {
            return "Pong";
        }
    }
}
