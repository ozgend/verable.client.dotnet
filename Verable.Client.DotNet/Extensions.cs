using System;
using Verable.Client.DotNet.Contracts;

namespace Verable.Client.DotNet
{
    internal static class Extensions
    {
        private static readonly Random _random = new Random();

        public static string GetSingleBeaconEndpoint(this Settings config)
        {
            return config.Target?.Count== 1
                ? config.Target[0]
                : config.Target?[_random.Next(config.Target.Count)];

        }
    }
}