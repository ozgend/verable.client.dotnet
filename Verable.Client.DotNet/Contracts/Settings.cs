using System.Collections.Generic;

namespace Verable.Client.DotNet.Contracts
{
    public class Settings
    {
        public List<string> Target { get; set; }
        public string Version { get; set; }
    }
}