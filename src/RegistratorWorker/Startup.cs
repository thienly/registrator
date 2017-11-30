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

        public Uri ServerAddress { get; set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var configFile = Configuration.GetValue<string>("configFile");            
            var fileConfig = JsonConvert.DeserializeObject<RegistrationConfig>(File.ReadAllText(configFile));
            services.AddOptions();
            services.Configure<RegistrationConfig>(config =>
            {
                config.Host = fileConfig.Host;
                config.Consul = fileConfig.Consul;
                config.DockerHost = fileConfig.DockerHost;
                config.IntervalCheck = fileConfig.IntervalCheck;
                config.Name = fileConfig.Name;
                config.Role = fileConfig.Role;
                config.DeregisterServiceAfter = fileConfig.DeregisterServiceAfter;
                config.TimeOut = fileConfig.TimeOut;
            });
            
            services.AddSingleton<IConsulUtilities, ConsulUtilities>();
            services.AddSingleton<IEnumerable<IDockerContainersCollector>>(provider =>
            {
                return fileConfig.DockerHost
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

            var featureCollection = app.Properties["server.Features"] as FeatureCollection;
            var serverAddressesFeature = featureCollection.Get<IServerAddressesFeature>();
            var address = serverAddressesFeature.Addresses.First();
            ServerAddress = new Uri(address);

            applicationLifetime.ApplicationStarted.Register(async ()=> await OnApplicationStarted(app));
            applicationLifetime.ApplicationStopped.Register(OnApplicationStopped);
            app.UseMvc();
        }

        public async Task OnApplicationStarted(IApplicationBuilder app)
        {
            var configFile = Configuration.GetValue<string>("configFile");
            var configuration = JsonConvert.DeserializeObject<RegistrationConfig>(File.ReadAllText(configFile));
            var consulUtilities = app.ApplicationServices.GetService<IConsulUtilities>();            
            //check itself
            using (var consulClient = consulUtilities.CreateConsulClient())
            {
                await consulClient.Agent.ServiceRegister(new AgentServiceRegistration()
                {
                    Address = $"{ServerAddress.Scheme}://{ServerAddress.Host}",
                    Check = new AgentServiceCheck()
                    {
                        HTTP = $"{ServerAddress.Scheme}://{ServerAddress.Host}:{ServerAddress.Port}/api/health/status",
                        Interval = TimeSpan.FromMilliseconds(configuration.IntervalCheck),
                        DeregisterCriticalServiceAfter =
                            TimeSpan.FromMilliseconds(configuration.DeregisterServiceAfter),
                        Timeout = configuration.TimeOut == 0
                            ? TimeSpan.FromMinutes(5)
                            : TimeSpan.FromMilliseconds(configuration.TimeOut),
                        Status = HealthStatus.Passing
                    },
                    Name = configuration.Name,
                    ID = configuration.Name + "_" + Guid.NewGuid(),
                    Tags = new[] {configuration.Role},
                    Port = ServerAddress.Port,
                    EnableTagOverride = true
                });
            }
            // collect data for the first time.
            foreach (var dockerHost in configuration.DockerHost)
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
