using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Api.Jobs.DTOs
{
    public class JobScheduledResponseDto
    {
        [JsonProperty(Required = Required.Always)]
        public string JobId { get; set; } = string.Empty;
    }
}
