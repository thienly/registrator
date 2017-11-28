using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Consul;
using Xunit;

namespace RegistratorWorkerTests
{
    public class ConsulTest
    {
        [Fact]
        public async Task QueryKV()
        {
            await AddServiceToConsul();
            using (var consulClient = new ConsulClient(cfg => { cfg.Address = new Uri($"http://10.0.19.110:8500"); }))
            {
                var queryResult = await consulClient.KV.Get("Eton/");
                var queryResultResponse = queryResult.Response;
            }
        }

        private async Task AddServiceToConsul()
        {
             using (var consulClient = new ConsulClient(cfg => { cfg.Address = new Uri($"http://10.0.19.110:8500"); }))
            {
                await consulClient.Agent.ServiceRegister(new AgentServiceRegistration()
                {
                    Address = "http://google.com",
                    Check = new AgentServiceCheck()
                    {

                    },
                    EnableTagOverride = true,
                    ID = Guid.NewGuid().ToString(),
                    Name = "Service1",
                    Port = 8080,
                    Tags = new[] {"Worker"}
                });
            }
        }

        [Fact]
        public async Task Reset_Services()
        {
            using (var consulClient = new ConsulClient(cfg => { cfg.Address = new Uri($"http://10.0.19.110:8500"); }))
            {
                var queryResult = await consulClient.Agent.Services();
                foreach (var responseValue in queryResult.Response.Values)
                {
                    if (responseValue.Service != "consul")
                        await consulClient.Agent.ServiceDeregister(responseValue.ID);
                }
            }
        }
    }
}
