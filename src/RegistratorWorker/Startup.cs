using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Docker.DotNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RegistratorWorker.Collector;
using RegistratorWorker.Common;
using RegistratorWorker.Consul;

namespace RegistratorWorker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<RegistrationConfig>(Configuration);
            
            services.AddSingleton<IConsulUtilities, ConsulUtilities>();
            services.AddSingleton<IEnumerable<IDockerContainersCollector>>(provider =>
            {
                var dockerHost = JsonConvert.DeserializeObject<List<string>>(Environment.GetEnvironmentVariable("DockerHost"));
                return dockerHost
                    .Select(x => new DockerContainersCollector(new DockerClientConfiguration(new Uri(x)))).ToList();
            });
            services.AddSingleton<IntervalCheck, IntervalCheck>();            
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            applicationLifetime.ApplicationStarted.Register(async ()=> await OnApplicationStarted(app));
            applicationLifetime.ApplicationStopped.Register(OnApplicationStopped);
            app.UseMvc();
        }

        public async Task OnApplicationStarted(IApplicationBuilder app)
        {
            var config = new RegistrationConfig();
            var enumerator = Environment.GetEnvironmentVariables().GetEnumerator();
            var consulUtilities = app.ApplicationServices.GetService<IConsulUtilities>();
            while (enumerator.MoveNext())
            {
                string key = enumerator.Key.ToString();
                switch (key)
                {
                    case "Host":
                        config.Host = enumerator.Value.ToString();
                        break;
                    case "DeregisterServiceAfter":
                        config.DeregisterServiceAfter = int.Parse(enumerator.Value.ToString());
                        break;
                    case "Name":
                        config.Name = enumerator.Value.ToString();
                        break;
                    case "Role":
                        config.Role = enumerator.Value.ToString();
                        break;
                    case "IntervalCheck":
                        config.IntervalCheck = int.Parse(enumerator.Value.ToString());
                        break;
                    case "TimeOut":
                        config.TimeOut = int.Parse(enumerator.Value.ToString());
                        break;
                    case "Consul":
                        config.Consul = enumerator.Value.ToString();
                        break;
                    case "DockerHost":
                        config.DockerHost = enumerator.Value.ToString();
                        break;
                }
            }
            ////check itself
            using (var consulClient = consulUtilities.CreateConsulClient())
            {
                await consulClient.Agent.ServiceRegister(new AgentServiceRegistration()
                {
                    Address = $"http://{config.Host}",
                    Check = new AgentServiceCheck()
                    {
                        HTTP = $"http://{config.Host}:5000/api/health/status",
                        Interval = TimeSpan.FromMilliseconds(config.IntervalCheck),
                        DeregisterCriticalServiceAfter =
                            TimeSpan.FromMilliseconds(config.DeregisterServiceAfter),
                        Timeout = config.TimeOut == 0
                            ? TimeSpan.FromMinutes(5)
                            : TimeSpan.FromMilliseconds(config.TimeOut),
                        Status = HealthStatus.Passing
                    },
                    Name = config.Name,
                    ID = config.Name + "_" + Guid.NewGuid(),
                    Tags = new[] { config.Role },
                    Port = 5000,
                    EnableTagOverride = true
                });
            }
            // collect data for the first time.
            var dockerHosts = JsonConvert.DeserializeObject<List<string>>(Environment.GetEnvironmentVariable("DockerHost"));

            foreach (var dockerHost in dockerHosts)
            {                
                var collector = new DockerContainersCollector(new DockerClientConfiguration(new Uri(dockerHost)));
                var allRunningContainerAndIsCretedByService = await collector.GetAllRunningContainerThatIsCretedByService();
                await consulUtilities.PushData(allRunningContainerAndIsCretedByService.ToArray());
            }
            var intervalCheck =  app.ApplicationServices.GetService<IntervalCheck>();
            intervalCheck.Check();
        }
        public void OnApplicationStopped()
        {

        }
    }
    
}
