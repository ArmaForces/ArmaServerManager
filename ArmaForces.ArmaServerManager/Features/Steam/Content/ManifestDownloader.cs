using System;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Steam.Constants;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using BytexDigital.Steam.ContentDelivery.Models;
using BytexDigital.Steam.Core.Structs;
using Microsoft.Extensions.Logging;
using SteamKit2;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content
{
    internal class ManifestDownloader : IManifestDownloader
    {
        private readonly ISteamClient _steamClient;
        private readonly ILogger<ManifestDownloader> _logger;

        public ManifestDownloader(ISteamClient steamClient, ILogger<ManifestDownloader> logger)
        {
            _steamClient = steamClient;
            _logger = logger;
        }

        public async Task<Manifest> GetManifest(ContentItem contentItem, CancellationToken cancellationToken)
            => await _steamClient.ContentClient.GetManifestAsync(
                SteamConstants.ArmaAppId,
                SteamConstants.ArmaWorkshopDepotId,
                await GetManifestId(contentItem.Id),
                cancellationToken);

        /// <summary>
        /// TODO: Do it better
        /// </summary>
        private async Task<ManifestId> GetManifestId(uint contentItemId)
        {
            _logger.LogDebug("Downloading ManifestId for item {ContentItemId}", contentItemId);

            var errors = 0;

            // TODO: Use Polly
            while (true)
            {
                try
                {
                    return (await _steamClient.ContentClient.GetPublishedFileDetailsAsync(contentItemId))
                        .hcontent_file;
                }
                catch (TaskCanceledException exception)
                {
                    errors++;
                    LogManifestIdDownloadFailure(contentItemId, exception, errors);

                    if (errors >= SteamContentConstants.MaximumRetryCount)
                    {
                        throw CreateManifestDownloadException(errors, contentItemId, exception);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                catch (AsyncJobFailedException exception)
                {
                    errors++;
                    LogManifestIdDownloadFailure(contentItemId, exception, errors);

                    if (errors >= SteamContentConstants.MaximumRetryCount)
                    {
                        throw CreateManifestDownloadException(errors, contentItemId, exception);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }

        private void LogManifestIdDownloadFailure(
            uint contentItemId,
            Exception exception,
            int errors)
            => _logger.LogTrace(
                exception,
                "Failed to download ManifestId for item {ContentItemId}. Errors = {Number}",
                contentItemId,
                errors);

        private Exception CreateManifestDownloadException(
            int errors,
            uint contentItemId,
            Exception? innerException = null)
        {
            var newException = new Exception(
                $"{errors} errors while attempting to download manifest for {contentItemId}",
                innerException);

            if (innerException is null)
            {
                _logger.LogError(
                    newException,
                    "Could not download ManifestId for item {ContentItemId}",
                    contentItemId);
            }
            else
            {
                _logger.LogError(
                    newException,
                    "Could not download ManifestId for item {ContentItemId}, error message {Message}",
                    contentItemId,
                    innerException.Message);
            }

            return newException;
        }
    }
}
