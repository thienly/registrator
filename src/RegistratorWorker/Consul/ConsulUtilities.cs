using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Configuration;
using RegistratorWorker.Common;
  
namespace RegistratorWorker.Consul
{
    public class ConsulUtilities : IConsulUtilities
    {
        private readonly IConfiguration _configuration;
        public ConsulUtilities(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task PushData(IEnumerable<ContainerInfo> containerInfos)
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

        public async Task DeregisterService(IEnumerable<ContainerInfo> containerInfos)
        {
           using(var client = CreateConsulClient())
            {
                var queryResult = await client.Agent.Checks();
                if (queryResult.StatusCode == HttpStatusCode.OK)
                {
                    foreach (var containerInfo in containerInfos)
                    {
                        await client.Agent.ServiceDeregister(containerInfo.Id);
                    }
                }
            }
        }
        public ConsulClient CreateConsulClient()
        {
            var consulAddress = _configuration.GetValue<string>("consul");
            return new ConsulClient(cfg =>
            {
                cfg.Address = new Uri($"http://{consulAddress}"); 
            });
        }
        public ConsulClient CreateConsulClient(string consulAddress)
        {
            return new ConsulClient(cfg => { cfg.Address = new Uri($"http://{consulAddress}"); });
        }
    }
}
