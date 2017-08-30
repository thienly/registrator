using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Consul;
using System.IO;
using Docker.DotNet;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace etonregistrator.Controllers
{
    [Route("api/[controller]")]
    public class AliveController : Controller
    {
        private static int x = 0;
        [HttpGet]
        public int Alive()
        {
            x++;
            return x;
        }
    }
}
