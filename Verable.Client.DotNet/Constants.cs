using System;
using System.Collections.Generic;
using System.Text;

namespace Verable.Client.DotNet
{
    internal class Constants
    {
        public struct Config
        {
            public const string Verable = "Verable";
        }

        public struct Command
        {
            public const string Register = "REG";
            public const string Deregister = "DRG";
            public const string DiscoverOne = "DS1";
            public const string DiscoverN = "DSN";
            public const string DiscoverAll = "DSA";
            public const string Heartbeat = "HRB";
            public const string Seperator = "|";
        }
    }
}
