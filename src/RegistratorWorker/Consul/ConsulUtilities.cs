using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Options;
using RegistratorWorker.Common;
using Serilog;

namespace RegistratorWorker.Consul
{
    public class ConsulUtilities : IConsulUtilities
    {
        private readonly RegistrationConfig _configuration;
        public ConsulUtilities(IOptions<RegistrationConfig> configuration)
        {
            _configuration = configuration.Value;
        }
        public async Task PushData(IEnumerable<ContainerInfo> containerInfos)
        {
            var infos = containerInfos as IList<ContainerInfo> ?? containerInfos.ToList();
            using (var client = CreateConsulClient())
            {
                // get all services from cosul
                var queryResult = await client.Agent.Services();
                var latesRunningServices = queryResult.Response.Values.Select(x => x.ID).ToList();
                var moreServicesId = infos.Select(x => x.Id).ToList().Except(latesRunningServices).ToList();
                foreach (var containerInfo in infos.Where(x => moreServicesId.Contains(x.Id)))
                {
                    try
                    {
                        Log.Information("Registrator is checking for {@containerInfo}", containerInfo);
                        // internal check status before push to consul
                        var statusApi = $"http://{containerInfo.Host}:{containerInfo.Port}/api/health/status";
                        using (var httpChecker = new HttpClient())
                        {
                            var request = new HttpRequestMessage();
                            request.Method = HttpMethod.Get;
                            request.RequestUri = new Uri(statusApi);
                            var response = await httpChecker.SendAsync(request);
                            if (response.StatusCode != HttpStatusCode.OK)
                            {
                                Log.Warning("Registrator checked and {@containerInfo} this one is not good to push", containerInfo);
                                continue;
                            }        
                        }
                    }
                    catch(Exception)
                    {
                        continue;            
                    }
                
                    Log.Information("Registrator pushed {@containerInfo}",containerInfo);
                    var registration = new AgentServiceRegistration()
                    {                        
                        ID = containerInfo.Id,
                        Name = containerInfo.Name,
                        Address = containerInfo.Host,
                        Port = containerInfo.Port,
                        Tags = new[] { containerInfo.Name },
                        Check = new AgentServiceCheck()
                        {
                            HTTP = $"http://{containerInfo.Host}:{containerInfo.Port}/api/health/status",
                            Interval = TimeSpan.FromMilliseconds(10000),
                            DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(2),
                            Timeout = TimeSpan.FromMinutes(5),
                            Status = HealthStatus.Passing
                        },
                    };
                    await client.Agent.ServiceRegister(registration);
                }
            }
        }

        public async Task DeregisterService(IEnumerable<ContainerInfo> containerInfos)
        {
            using (var client = CreateConsulClient())
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
            var consulAddress = _configuration.Consul;
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
