using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Verable.Client;
using Verable.Client.DotNet;
using Verable.Client.DotNet.Contracts;

namespace Service1
{
    class Program
    {
        private static IConfigurationRoot _configuration;
        private static VerableBeacon _verableBeacon;

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                           .SetBasePath(Directory.GetCurrentDirectory())
                           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _configuration = builder.Build();

            _verableBeacon = VerableBeacon.Init(_configuration);

            Console.WriteLine("running");

            var definition = new ServiceDefinition { Endpoint = new Uri("http://localhost:6601"), Name = "Service2", Version = "1.0" };
            var id = await _verableBeacon.Register(definition);
            Console.WriteLine($"Register {definition.Name} id:{id}");

            var discoverOne = await _verableBeacon.DiscoverOne("Service2");
            Console.WriteLine($"Discover One: {JsonConvert.SerializeObject(discoverOne)}");

            var discoverN = await _verableBeacon.DiscoverN(new List<string> { "Service1", "Service2" });
            Console.WriteLine($"Discover N: {JsonConvert.SerializeObject(discoverOne)}");

            var discoverAll = await _verableBeacon.DiscoverAll();
            Console.WriteLine($"Discover All: {JsonConvert.SerializeObject(discoverAll)}");

            await _verableBeacon.Deregister();
            Console.WriteLine("Deregister");

            Console.WriteLine("awaiting...");
            Console.ReadLine();
        }
    }
}
