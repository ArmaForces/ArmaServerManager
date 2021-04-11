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
                await GetManifestId(contentItem),
                cancellationToken);

        /// <summary>
        /// TODO: Do it better
        /// </summary>
        private async Task<ManifestId> GetManifestId(ContentItem contentItem)
        {
            _logger.LogDebug("Downloading ManifestId for item {contentItemId}.", contentItem.Id);

            var errors = 0;

            while (true)
            {
                try
                {
                    return (await _steamClient.ContentClient.GetPublishedFileDetailsAsync(contentItem.Id))
                        .hcontent_file;
                }
                catch (TaskCanceledException exception)
                {
                    errors++;
                    LogManifestIdDownloadFailure(contentItem, exception, errors);

                    if (errors >= SteamContentConstants.MaximumRetryCount)
                    {
                        throw CreateManifestDownloadException(errors, contentItem, exception);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                catch (AsyncJobFailedException exception)
                {
                    errors++;
                    LogManifestIdDownloadFailure(contentItem, exception, errors);

                    if (errors >= SteamContentConstants.MaximumRetryCount)
                    {
                        throw CreateManifestDownloadException(errors, contentItem, exception);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }

        private void LogManifestIdDownloadFailure(
            ContentItem contentItem,
            Exception exception,
            int errors)
            => _logger.LogTrace(
                exception,
                "Failed to download ManifestId for item {contentItemId}. Errors = {number}.",
                contentItem.Id,
                errors);

        private Exception CreateManifestDownloadException(
            int errors,
            ContentItem contentItem,
            Exception? innerException = null)
        {
            var newException = new Exception(
                $"{errors} errors while attempting to download manifest for {contentItem.Id}",
                innerException);

            if (innerException is null)
            {
                _logger.LogError(
                    newException,
                    "Could not download ManifestId for item {contentItemId}.",
                    contentItem.Id);
            }
            else
            {
                _logger.LogError(
                    newException,
                    "Could not download ManifestId for item {contentItemId}, error message {message}.",
                    contentItem.Id,
                    innerException.Message);
            }

            return newException;
        }
    }
}
