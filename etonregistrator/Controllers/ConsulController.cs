using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Docker.DotNet;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using etonregistrator.Consul;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace etonregistrator.Controllers
{
    [Route("api/[controller]")]
    public class ConsulController : Controller
    {
        private readonly IServer _server;
        private readonly IConfiguration _configuration;
        private readonly IConnectToConsulUtilities _connectToConsulUtilities;
        public static ConcurrentDictionary<string,ContainerInfo> _cachedData = new ConcurrentDictionary<string,ContainerInfo>();
        public ConsulController(IServer server, IConfiguration configuration, IConnectToConsulUtilities connectToConsulUtilities)
        {
            _server = server;
            _configuration = configuration;
            _connectToConsulUtilities = connectToConsulUtilities;
        }
        [HttpGet]
        public async Task<IActionResult> RegisterAndDeregisterConsul()
        {
            var host = _configuration.GetValue<string>("host");
            var docker = _configuration.GetValue<string>("docker");
            var consulAddress = _configuration.GetValue<string>("consul");
           
            var client = new DockerClientConfiguration(new Uri(docker)).CreateClient();
            var filter = new Dictionary<string, IDictionary<string, bool>>();
            filter.Add("status", new Dictionary<string, bool>() { { "running", true } });
            var runningContainer = await client.Containers.ListContainersAsync(new Docker.DotNet.Models.ContainersListParameters()
            {
                Filters = filter
            });
            var serviceContainer = runningContainer.Where(x => x.Labels.ContainsKey("com.docker.swarm.service.name") && x.Ports.Any(p => p.PublicPort > 0)).Select(x => {
                var name = x.Labels.FirstOrDefault(l => l.Key == "com.docker.swarm.service.name").Value;
                var info = new ContainerInfo()
                {
                    Id = x.ID,
                    Host = host,
                    Name = name,
                    Port = x.Ports.FirstOrDefault(f => f.PublicPort > 0).PublicPort
                };
                return info; 
            }).ToList();
            if (!_cachedData.Any())
            {
                serviceContainer.ForEach(x => x.Status = ServiceAction.Register);
                await _connectToConsulUtilities.ConnectToConsul(serviceContainer);
                foreach (var item in serviceContainer)
                {
                    _cachedData.AddOrUpdate(item.Id, item, (k, v) => v);
                }
            }
            else
            {
                //deregister
                var listofDeregister = _cachedData.Except(serviceContainer.Select(s=>new KeyValuePair<string,ContainerInfo>(s.Id,s)).ToList(), new ContainerInfo());
                var de = listofDeregister.Select(x => x.Value).ToList();
                if (listofDeregister.Any())
                {
                    foreach (var item in de)
                    {
                        item.Status = ServiceAction.Deregister;
                    }
                    await _connectToConsulUtilities.ConnectToConsul(de);
                }
                var listofRegister = serviceContainer.Except(_cachedData.Values, new ContainerInfo()).ToList();
                if (listofRegister.Any())
                {
                    listofRegister.ForEach(r => r.Status = ServiceAction.Register);
                    await _connectToConsulUtilities.ConnectToConsul(listofRegister);
                }
                de.ForEach(d =>
                  {
                      ContainerInfo outData;
                      _cachedData.Remove(d.Id, out outData);
                  });
                listofRegister.ForEach(r => _cachedData.AddOrUpdate(r.Id, r, (k, v) =>
                {
                    v.Status = ServiceAction.Default;
                    return v;
                }));
            }
            return Ok();
        }
        [Route("Reset")]
        [HttpGet]
        public async Task Reset()
        {
            await _connectToConsulUtilities.DeregisterServiceThatAlreadyRegisterBy(_cachedData.Values);
            _cachedData.Clear();
        }
        [Route("Data")]
        [HttpGet]
        public IActionResult GetData()
        {
            return new JsonResult(_cachedData.Values);
        }
        
    }
}
