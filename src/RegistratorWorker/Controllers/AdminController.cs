using System;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Mvc;

namespace RegistratorWorker.Controllers
{
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        [Route("ConsulReset")]
        [HttpGet]
        public async Task<IActionResult> ResetConsulServices()
        {
            using (var consulClient = new ConsulClient(cfg => { cfg.Address = new Uri($"http://{Environment.GetEnvironmentVariable("Consul")}"); }))
            {
                var queryResult = await consulClient.Agent.Services();
                foreach (var responseValue in queryResult.Response.Values)
                {
                    if (responseValue.Service != "consul")
                        await consulClient.Agent.ServiceDeregister(responseValue.ID);
                }
                var result = await consulClient.Agent.Checks();
                foreach (var responseValue in result.Response.Values)
                {
                    await consulClient.Agent.CheckDeregister(responseValue.CheckID);
                }
            }
            return Ok();
        }
    }
}
