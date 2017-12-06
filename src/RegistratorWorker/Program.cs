using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using Serilog;
using Serilog.Events;

namespace RegistratorWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.LiterateConsole()
                .CreateLogger();
            try
            {
                Log.Warning("Starting web host");
                BuildWebHost(args).Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, 5000);
                }).UseSerilog()
                .Build();
    }
}
