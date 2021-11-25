using Newtonsoft.Json;

namespace ArmaForces.Arma.Server.Features.Servers.DTOs
{
    public class ServerStatus
    {
        public ServerStatus(
            ServerStatusEnum serverStatusEnum = ServerStatusEnum.Stopped,
            string? name = null,
            string? modsetName = null,
            string? mapName = null,
            int? playersCount = null,
            int? playersMaxCount = null,
            int? port = null,
            int? headlessClientsCount = null)
        {
            Status = serverStatusEnum;
            Name = name;
            ModsetName = modsetName;
            Map = mapName;
            Players = playersCount;
            PlayersMax = playersMaxCount;
            Port = port;
            HeadlessClientsConnected = headlessClientsCount;
        }

        [JsonProperty(Required = Required.Always)]
        public ServerStatusEnum Status { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? Name { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? ModsetName { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? Map { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Players { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PlayersMax { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Port {get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? HeadlessClientsConnected { get; }
    }
}
