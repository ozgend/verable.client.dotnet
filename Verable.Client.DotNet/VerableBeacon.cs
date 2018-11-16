using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Verable.Client.DotNet.Contracts;

namespace Verable.Client.DotNet
{
  public class VerableBeacon
  {
    private Settings _settings = new Settings();
    private IPAddress _lastSuccessEndpointIp;
    private ushort _lastSuccessPort;
    private static VerableBeacon _instance;
    public string RegistrationId;

    public static VerableBeacon Init(IConfiguration configuration)
    {
      if (_instance == null)
      {
        _instance = new VerableBeacon(configuration);
      }

      return _instance;
    }

    public VerableBeacon(IConfiguration configuration)
    {
      configuration.Bind("Verable", _settings);
    }

    public async Task<string> Register(ServiceDefinition definition, bool force = false)
    {
      if (!string.IsNullOrEmpty(RegistrationId))
      {
        Console.WriteLine($"Already registered with id: {RegistrationId}");

        if (!force)
        {
          return RegistrationId;
        }

        await Deregister(RegistrationId);
      }
      var request = Pack(Constants.Command.Register, definition, true);
      var response = await Send(request);
      RegistrationId = Unpack<string>(response.Response);
      return RegistrationId;
    }

    public async Task Deregister(string registrationId = null)
    {
      var request = Pack(Constants.Command.Deregister, registrationId ?? RegistrationId);
      await Send(request);
      RegistrationId = null;
    }

    public async Task<List<ServiceDefinition>> DiscoverOne(string name)
    {
      var request = Pack(Constants.Command.DiscoverOne, name);
      var response = await Send(request);
      var result = Unpack<List<ServiceDefinition>>(response.Response, true);
      return result;
    }

    public async Task<Dictionary<string, List<ServiceDefinition>>> DiscoverAll()
    {
      var request = Pack(Constants.Command.DiscoverAll);
      var response = await Send(request);
      var result = Unpack<Dictionary<string, List<ServiceDefinition>>>(response.Response, true);
      return result;
    }

    private async Task<TcpResponse> Send(string data)
    {
      var tcpResult = await Send(_lastSuccessEndpointIp, _lastSuccessPort, data);
      if (tcpResult.Success)
      {
        return tcpResult;
      }

      foreach (var target in _settings.Target)
      {
        Console.WriteLine($"+ will resolve {target}");

        IEnumerable<IPAddress> ipAddressList;
        var beaconEndpoint = new Uri(target);

        try
        {
          ipAddressList = Dns.GetHostAddresses(beaconEndpoint.DnsSafeHost).Where(ip => ip.AddressFamily != AddressFamily.InterNetworkV6);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"- cannot resolve host {beaconEndpoint}");
          continue;
        }

        foreach (var ipAddress in ipAddressList)
        {
          Console.WriteLine($"   >> will try {target} | {beaconEndpoint} -> [selected] {ipAddress} {ipAddress.AddressFamily.ToString()}");
          tcpResult = await Send(ipAddress, (ushort)beaconEndpoint.Port, data);

          if (tcpResult.Success)
          {
            Console.WriteLine($"   >> success: {beaconEndpoint} -> [selected] {ipAddress}");
            return tcpResult;
          }
          else
          {
            Console.WriteLine($"   >> failed:  {beaconEndpoint} -> [selected] {ipAddress}");
          }
        }
      }

      return tcpResult;
    }

    private async Task<TcpResponse> Send(IPAddress ipAddress, ushort port, string data)
    {
      var tcpResponse = new TcpResponse
      {
        Target = $"{ipAddress}:{port}"
      };

      try
      {
        Console.WriteLine($">>>> endpoint {ipAddress} {ipAddress.AddressFamily.ToString()}:{port}");

        var client = new TcpClient();

        await client.ConnectAsync(ipAddress, port);

        var networkStream = client.GetStream();
        var streamWriter = new StreamWriter(networkStream);
        var streamReader = new StreamReader(networkStream);

        streamWriter.AutoFlush = true;

        await streamWriter.WriteAsync(data);

        tcpResponse.Response = await streamReader.ReadToEndAsync();
        tcpResponse.Success = true;
        _lastSuccessEndpointIp = ipAddress;
        _lastSuccessPort = port;

        client.Close();

        return tcpResponse;
      }
      catch (Exception ex)
      {
        tcpResponse.Error = ex;
        //Console.WriteLine(ex);
        return tcpResponse;
      }
    }

    class TcpResponse
    {
      public string Target { get; set; }
      public string Response { get; set; }
      public Exception Error { get; set; }
      public bool Success { get; set; }
    }


    private string Pack(string command, object data = null, bool requireSerialization = false)
    {
      var packet = new BeaconPacket
      {
        Command = command,
        Serialized = requireSerialization
      };

      if (data != null)
      {
        packet.Data = requireSerialization
            ? JsonConvert.SerializeObject(data)
            : (string)data;
      }

      var stringified = packet.ToString();
      var plainTextBytes = Encoding.UTF8.GetBytes(stringified);
      return Convert.ToBase64String(plainTextBytes);
    }

    private TResult Unpack<TResult>(string encoded, bool expectSerialized = false)
    {
      if (string.IsNullOrEmpty(encoded))
      {
        return default(TResult);
      }

      var bytes = Convert.FromBase64String(encoded);
      var stringified = Encoding.UTF8.GetString(bytes);
      TResult data;
      if (expectSerialized)
      {
        data = JsonConvert.DeserializeObject<TResult>(stringified);
      }
      else
      {
        data = (TResult)Convert.ChangeType(stringified, typeof(TResult));
      }
      return data;
    }
  }
}
