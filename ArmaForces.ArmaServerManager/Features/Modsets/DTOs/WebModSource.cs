using System.Text.Json.Serialization;
using ArmaForces.Arma.Server.Features.Mods;

namespace ArmaForces.ArmaServerManager.Features.Modsets.DTOs;

/// <summary>
/// Describes the source of the mod as reported by website.
/// </summary>
/// <seealso cref="ModSource"/>
public enum WebModSource
{
    /// <inheritdoc cref="ModSource.SteamWorkshop"/>
    /// <seealso cref="ModSource.SteamWorkshop"/>
    [JsonStringEnumMemberName("steam_workshop")]
    SteamWorkshop,
    
    /// <inheritdoc cref="ModSource.Directory"/>
    /// <seealso cref="ModSource.Directory"/>
    Directory
}