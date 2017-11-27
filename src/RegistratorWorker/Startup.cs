using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
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
            //applicationLifetime.ApplicationStopped.Register(OnApplicationStopped);
            app.UseMvc();
        }

        public void OnApplicationStarted()
        {
            var consulUtilities = new ConsulUtilities(Configuration);
            using (var consulClient = consulUtilities.CreateConsulClient())
            {
                consulClient.KV.Put(new KVPair("EtonRegistrator")
                {
                    Value = Encoding.UTF8.GetBytes("AAAAA")
                }).Wait();

            }
            //Auto Add to consul if this is manager
            //var serverAddress = Configuration.GetValue<string>("manager");
            //if (string.IsNullOrEmpty(serverAddress))
            //    throw new ApplicationException("Can not start the application due to missing server parameter");
            //using (var httpClient = new HttpClient())
            //{
            //    httpClient.BaseAddress = new Uri($"http://{serverAddress}");
            //}
        }

        public void OnApplicationStopped()
        {

        }
    }
}
