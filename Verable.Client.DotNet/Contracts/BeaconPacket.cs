using Newtonsoft.Json;

namespace Verable.Client.DotNet.Contracts
{
    public class BeaconPacket
    {
        public string Command { get; set; }
        public string Data { get; set; }
        public bool Serialized { get; set; }

        public override string ToString()
        {
            return $"{Command}{Constants.Command.Seperator}{(Serialized ? 1 : 0)}{Constants.Command.Seperator}{Data}";
        }
    }
}
