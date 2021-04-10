using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ArmaForces.Arma.Server.Features.Servers.DTOs
{
    public class ServerStatus
    {
        private readonly IDedicatedServer? _dedicatedServer;
        private readonly A2SInfo? _serverInfo;

        public ServerStatus()
        {
        }

        public ServerStatus(IDedicatedServer dedicatedServer, A2SInfo? serverInfo)
        {
            _dedicatedServer = dedicatedServer;
            _serverInfo = serverInfo;
        }

        public static async Task<ServerStatus> GetServerStatus(
            IDedicatedServer dedicatedServer,
            CancellationToken cancellationToken)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Loopback, dedicatedServer.Port + 1);
            var serverInfo = await A2SInfo.GetServerInfoAsync(ipEndPoint, cancellationToken);

            return new ServerStatus(dedicatedServer, serverInfo);
        }

        [JsonProperty(Required = Required.Always)]
        public ServerStatusEnum Status => GetServerStatusEnum();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? Name => _serverInfo?.Name;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? ModsetName => _dedicatedServer?.Modset.Name;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? Map => _serverInfo?.Map;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Players => _serverInfo?.Players;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PlayersMax => _serverInfo?.MaxPlayers;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Port => _dedicatedServer?.Port;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? HeadlessClientsConnected => _dedicatedServer?.HeadlessClientsConnected;

        private ServerStatusEnum GetServerStatusEnum()
        {
            if (!(_serverInfo is null))
            {
                return ServerStatusEnum.Started;
            }

            if (!_dedicatedServer?.IsServerStopped ?? false)
            {
                return ServerStatusEnum.Starting;
            }

            return ServerStatusEnum.Stopped;
        }
    }
}
