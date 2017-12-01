using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Options;
using RegistratorWorker.Collector;
using RegistratorWorker.Consul;

namespace RegistratorWorker.Common
{
    public class IntervalCheck
    {
        private readonly RegistrationConfig _config;
        private readonly IConsulUtilities _consulUtilities;
        private readonly IEnumerable<IDockerContainersCollector> _containersCollector;

        public IntervalCheck(IEnumerable<IDockerContainersCollector> containersCollector,
            IConsulUtilities consulUtilities, IOptions<RegistrationConfig> optionsConfig)
        {
            _containersCollector = containersCollector;
            _consulUtilities = consulUtilities;
            _config = optionsConfig.Value;
        }

        public void Check()
        {
            var timer = new RegisterTimer(async () => await LogicWhenPerformCheck(_config), TimeSpan.FromMilliseconds(_config.IntervalCheck));
            timer.Start();
        }

        private async Task LogicWhenPerformCheck(RegistrationConfig config)
        {
            // perform check health and deregister if service is critical status
            // clean up internal data
            await DeregisterCriticalService();
            foreach (var dockerContainersCollector in _containersCollector)
            {
                var allRunningContainerThatIsNotRegister = await dockerContainersCollector.GetAllRunningContainerThatIsNotRegister();
                await _consulUtilities.PushData(allRunningContainerThatIsNotRegister);
            }
        }

        private async Task DeregisterCriticalService()
        {
            using (var consulClient = _consulUtilities.CreateConsulClient())
            {
                var result = await consulClient.Agent.Checks();
                foreach (var responseValue in result.Response.Values.Where(r => r.Status == HealthStatus.Critical))
                {
                    await consulClient.Agent.ServiceDeregister(responseValue.ServiceID);
                }
            }
        }

    }
}
