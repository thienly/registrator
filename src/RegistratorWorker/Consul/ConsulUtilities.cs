﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RegistratorWorker.Collector;
using RegistratorWorker.Common;

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
                var queryResult = await client.Agent.Checks();
                if (queryResult.StatusCode == HttpStatusCode.OK)
                {
                    foreach (var containerInfo in infos.ToList())
                    {
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
            CollectorInternalData.Current.AddRange(infos);

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
