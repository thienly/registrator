using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace RegistratorWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://10.0.19.160:5000")
                .UseStartup<Startup>()
                .Build();
    }
}
