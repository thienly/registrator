using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Consul;
using System.Net;
using etonregistrator.Controllers;

namespace etonregistrator
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
            services.AddSingleton<IConnectToConsulUtilities, ConnecToConsulUtilities>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            var consulUtilities = serviceProvider.GetService<IConnectToConsulUtilities>();
            applicationLifetime.ApplicationStopping.Register(()=> consulUtilities.DeregisterServiceThatAlreadyRegisterBy(ConsulController._cachedData.Values).GetAwaiter().GetResult());
            app.UseMvc();
        }
        private void ApplicationShutDownCallback()
        {
            var consulAddress = Configuration.GetValue<string>("consul");
            using (var client = new ConsulClient(cfg =>
            {
                cfg.Address = new Uri($"http://{consulAddress}");

            }))
            {
                var queryResult = client.Agent.Checks().GetAwaiter().GetResult();
                if (queryResult.StatusCode == HttpStatusCode.OK)
                {
                    foreach (var containerInfo in ConsulController._cachedData)
                    {
                        client.Agent.ServiceDeregister(containerInfo.Key).GetAwaiter().GetResult();
                    }
                }
            }
        }
    }
    
}
