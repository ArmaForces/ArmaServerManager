using System;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Steam.Constants;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using BytexDigital.Steam.ContentDelivery.Models;
using BytexDigital.Steam.Core.Structs;
using Microsoft.Extensions.Logging;
using Polly;
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
        {
            await _steamClient.EnsureConnected(cancellationToken);

            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(15));

            return await _steamClient.ContentClient.GetManifestAsync(
                appId: SteamConstants.ArmaAppId,
                depotId: SteamConstants.ArmaWorkshopDepotId,
                manifestId: await GetManifestId(contentItem, cancellationTokenSource.Token),
                cancellationToken: cancellationTokenSource.Token);
        }
        
        private async Task<ManifestId> GetManifestId(ContentItem contentItem, CancellationToken cancellationToken)
        {
            var asyncJobFailedPolicy = Policy<ulong>
                .Handle<AsyncJobFailedException>()
                .WaitAndRetryAsync(
                    retryCount: SteamContentConstants.MaximumRetryCount,
                    sleepDurationProvider: _ => TimeSpan.FromSeconds(5), 
                    onRetry: (result, _, _) => LogManifestIdDownloadFailure(result.Exception, contentItem));

            var taskCanceledPolicy = Policy<ulong>
                .Handle<TaskCanceledException>()
                .FallbackAsync(Task.FromCanceled<ulong>);

            var policy = Policy.WrapAsync(asyncJobFailedPolicy, taskCanceledPolicy);

            _logger.LogDebug("Downloading ManifestId for item {ContentItemId}", contentItem.Id);
            
            var result = await policy.ExecuteAndCaptureAsync(async token =>
                (await _steamClient.ContentClient.GetPublishedFileDetailsAsync(contentItem.Id, token)).hcontent_file,
                cancellationToken);

            return result.Outcome == OutcomeType.Successful
                ? result.Result
                : throw CreateManifestDownloadException(contentItem, result.FinalException);
        }

        private void LogManifestIdDownloadFailure(Exception exception, ContentItem contentItem)
            => _logger.LogTrace(
                exception,
                "Failed to download ManifestId for item {ContentItemId}",
                contentItem.Id);

        private Exception CreateManifestDownloadException(ContentItem contentItem, Exception? innerException = null)
        {
            var newException = new Exception(
                $"Failed while attempting to download manifest for {contentItem.Id}",
                innerException);

            if (innerException is null)
            {
                _logger.LogError(
                    newException,
                    "Could not download ManifestId for item {ContentItemId}",
                    contentItem.Id);
            }
            else
            {
                _logger.LogError(
                    innerException,
                    "Could not download ManifestId for item {ContentItemId}, error message {Message}",
                    contentItem.Id,
                    newException.Message);
            }

            return newException;
        }
    }
}
