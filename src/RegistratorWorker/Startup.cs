using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddSingleton<IConsulUtilities, ConsulUtilities>();
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

            applicationLifetime.ApplicationStarted.Register(OnApplicationStarted);
            applicationLifetime.ApplicationStopped.Register(OnApplicationStopped);
            app.UseMvc();
        }

        public void OnApplicationStarted()
        {
            var role = Configuration.GetValue<string>("role");
            var name = Configuration.GetValue<string>("name");
            var host = Configuration.GetValue<string>("host");
            var intervalCheck = Configuration.GetValue<string>("intervalCheck");
            if (String.IsNullOrEmpty(intervalCheck))
                intervalCheck = "5";
            var consulUtilities = new ConsulUtilities(Configuration);
            using (var consulClient = consulUtilities.CreateConsulClient())
            {
            
                var task = Task.Run(() => consulClient.Agent.ServiceRegister(new AgentServiceRegistration()
                {
                    Address = $"http://{host}",
                    Check = new AgentServiceCheck()
                    {
                        HTTP = $"http://{host}:5000/api/health/status",
                        Interval = TimeSpan.FromSeconds(int.Parse(intervalCheck)),
                        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(2),
                        Timeout = TimeSpan.FromMinutes(10)
                    },
                    Name = name ?? Guid.NewGuid().ToString(),
                    ID = Guid.NewGuid().ToString(),
                    Tags = new[] {"manager"},
                    Port = 5000,
                }));
                task.Wait();
            }

        }

        public void OnApplicationStopped()
        {

        }
    }
}
