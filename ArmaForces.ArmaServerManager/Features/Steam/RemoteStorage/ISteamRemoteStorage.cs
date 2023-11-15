using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Steam.RemoteStorage;

/// <summary>
/// https://partner.steamgames.com/doc/webapi/ISteamRemoteStorage
/// </summary>
public interface ISteamRemoteStorage
{
    /// <summary>
    /// Retrieves details of published file (workshop item).
    /// </summary>
    /// <param name="publishedFileIds">IDs of workshop items to retrieve details for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Result<PublishedFileDetails[]>> GetPublishedFileDetails(IEnumerable<ulong> publishedFileIds, CancellationToken cancellationToken);
}

internal class SteamRemoteStorage : ISteamRemoteStorage
{
    private const string GetPublishedFileDetailsUrl =
        "https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/";
    
    private readonly HttpClient _httpClient;

    public SteamRemoteStorage(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<Result<PublishedFileDetails[]>> GetPublishedFileDetails(IEnumerable<ulong> publishedFileIds, CancellationToken cancellationToken)
    {
        var idsQueryList = publishedFileIds.ToList();

        var requestedItems = new Dictionary<string, string>
        {
            {"itemcount", idsQueryList.Count.ToString()}
        };
        
        for (var i = 0; i < idsQueryList.Count; i++)
        {
            requestedItems.Add($"publishedfileids[{i}]", idsQueryList[i].ToString());
        }

        var requestContent = new FormUrlEncodedContent(requestedItems);

        var response = await _httpClient.PostAsync(GetPublishedFileDetailsUrl, requestContent, cancellationToken);

        var responseData = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (!response.IsSuccessStatusCode) return Result.Failure<PublishedFileDetails[]>(responseData);

        var publishedFileDetails = JsonSerializer.Deserialize<JsonObject>(responseData)?.Single().Value?.AsObject()
            ["publishedfiledetails"].Deserialize<PublishedFileDetailsRaw[]>();

        if (publishedFileDetails is null) return Result.Failure<PublishedFileDetails[]>($"Failed parsing published file details. Response: {responseData}");

        return Result.Success(publishedFileDetails
            .Select(x => new PublishedFileDetails
            {
                PublishedFileId = long.Parse(x.PublishedFileId),
                Title = x.Title,
                LastUpdatedAt = DateTimeOffset.FromUnixTimeSeconds(x.TimeUpdated)
            })
            .ToArray());
    }

    private record PublishedFileDetailsRaw
    {
        [JsonPropertyName("publishedfileid")]
        public string PublishedFileId { get; init; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; init; } = string.Empty;
    
        [JsonPropertyName("time_updated")]
        public long TimeUpdated { get; init; }
    }
}

public record PublishedFileDetails
{
    public long PublishedFileId { get; init; }

    public string Title { get; init; } = string.Empty;
    
    public DateTimeOffset LastUpdatedAt { get; init; }
}