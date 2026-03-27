using System.Text.Json.Serialization;

namespace ArmaForces.ArmaServerManager.Features.Webhooks.DTOs;

internal record DiscordWebhook
{
    [JsonPropertyName("username")]
    public string UserName { get; init; }
    
    public string Content { get; init; }
}