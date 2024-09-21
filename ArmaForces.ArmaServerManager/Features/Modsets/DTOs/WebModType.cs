using System.Text.Json.Serialization;
using ArmaForces.Arma.Server.Features.Mods;

namespace ArmaForces.ArmaServerManager.Features.Modsets.DTOs;

/// <summary>
/// Describes type of the mod as reported by website.
/// </summary>
/// <seealso cref="ModType"/>
public enum WebModType
{
    /// <inheritdoc cref="ModType.ServerSide"/>
    /// <seealso cref="ModType.ServerSide"/>
    [JsonStringEnumMemberName("server_side")]
    ServerSide,

    /// <inheritdoc cref="ModType.Required"/>
    /// <seealso cref="ModType.Required"/>
    Required,

    /// <inheritdoc cref="ModType.Optional"/>
    /// <seealso cref="ModType.Optional"/>
    Optional,

    /// <inheritdoc cref="ModType.ClientSide"/>
    /// <seealso cref="ModType.ClientSide"/>
    [JsonStringEnumMemberName("client_side")]
    ClientSide
}