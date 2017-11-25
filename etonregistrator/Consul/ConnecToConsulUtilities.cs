using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Consul;
using etonregistrator.Controllers;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace etonregistrator.Consul
{
    public class ConnecToConsulUtilities : IConnectToConsulUtilities
    {
        private readonly IConfiguration _configuration;
        public ConnecToConsulUtilities(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task ConnectToConsul(IEnumerable<ContainerInfo> containerInfos)
        {
            using (var client = CreateConsulClient())
            {
                var queryResult = await client.Agent.Checks();
                if (queryResult.StatusCode == HttpStatusCode.OK)
                {
                    foreach (var containerInfo in containerInfos)
                    {
                        if (containerInfo.Status == ServiceAction.Register)
                        {
                            var registration = new AgentServiceRegistration()
                            {
                                ID = containerInfo.Id,
                                Name = containerInfo.Name,
                                Address = containerInfo.Host,
                                Port = containerInfo.Port,
                                Tags = new[] { containerInfo.Name }
                            };
                            await client.Agent.ServiceRegister(registration);
                        }
                        if (containerInfo.Status == ServiceAction.Deregister)
                        {
                            await client.Agent.ServiceDeregister(containerInfo.Id);
                        }
                    }
                }
            }
        }

        public async Task DeregisterServiceThatAlreadyRegisterBy(IEnumerable<ContainerInfo> containerInfos)
        {
           using(var client = CreateConsulClient())
            {
                var queryResult = await client.Agent.Checks();
                if (queryResult.StatusCode == HttpStatusCode.OK)
                {
                    foreach (var containerInfo in ConsulController._cachedData)
                    {
                        await client.Agent.ServiceDeregister(containerInfo.Key);
                    }
                }
            }
        }
        private ConsulClient CreateConsulClient()
        {
            var consulAddress = _configuration.GetValue<string>("consul");
            return new ConsulClient(cfg => { cfg.Address = new Uri($"http://{consulAddress}"); });
        }
    }
}
