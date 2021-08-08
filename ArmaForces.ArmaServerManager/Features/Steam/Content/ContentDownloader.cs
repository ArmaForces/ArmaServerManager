using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Steam.Constants;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using ArmaForces.ArmaServerManager.Features.Steam.Content.Exceptions;
using BytexDigital.Steam.ContentDelivery.Exceptions;
using BytexDigital.Steam.ContentDelivery.Models.Downloading;
using BytexDigital.Steam.Core.Enumerations;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content
{
    /// <inheritdoc />
    public class ContentDownloader : IContentDownloader
    {
        private static TimeSpan ProgressLogInterval { get; } = TimeSpan.FromSeconds(5);
        
        private readonly string _modsDirectory;
        private readonly ISteamClient _steamClient;
        private readonly ILogger<ContentDownloader> _logger;

        public ContentDownloader(
            ISettings settings,
            ISteamClient steamClient,
            ILogger<ContentDownloader> contentDownloader)
            : this(steamClient, settings.ModsDirectory!,
                contentDownloader)
        {
        }

        /// <inheritdoc cref="ContentDownloader" />
        /// <param name="steamClient">Client used for connection.</param>
        /// <param name="modsDirectory">Directory where mods should be stored.</param>
        /// <param name="logger">Logger</param>
        private ContentDownloader(
            ISteamClient steamClient,
            string modsDirectory,
            ILogger<ContentDownloader> logger)
        {
            _steamClient = steamClient;
            _logger = logger;
            _modsDirectory = modsDirectory;
        }

        public async Task<List<Result<IMod>>> DownloadOrUpdateMods(
            IReadOnlyCollection<IMod> mods,
            CancellationToken cancellationToken)
        {
            var workshopMods = mods
                .Where(x => x.Source == ModSource.SteamWorkshop)
                .ToList();
            
            _logger.LogTrace("Downloading {Count} items: {@ItemsIds}", workshopMods.Count, workshopMods);

            await _steamClient.EnsureConnected(cancellationToken);
            
            var results = new List<Result<IMod>>();
            foreach (var mod in workshopMods)
            {
                if (cancellationToken.IsCancellationRequested) CancelDownload();
                
                await DownloadOrUpdate(mod.AsContentItem(), cancellationToken)
                    .Bind(downloadedItem => UpdateModData(mod, downloadedItem))
                    .TapOnBoth(modUpdateResult => results.Add(modUpdateResult));
            }

            return results;
        }

        private async Task<Result<ContentItem>> DownloadOrUpdate(
            ContentItem contentItem,
            CancellationToken cancellationToken)
            => await Download(contentItem, cancellationToken);

        /// <summary>
        /// Safely cancels download process.
        /// </summary>
        private void CancelDownload()
        {
            _logger.LogWarning("Download cancelled");
            throw new OperationCanceledException();
        }

        /// <summary>
        ///     Handles download process
        /// </summary>
        /// <param name="contentItem">Item to download.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        private async Task<Result<ContentItem>> Download(ContentItem contentItem, CancellationToken cancellationToken)
        {
            if (contentItem.ItemType == ItemType.App)
                throw new NotImplementedException("Downloading Arma 3 Server is not supported yet.");
            
            var downloadHandler = await GetDownloadHandler(contentItem);

            var contentDownloadHandler = new ContentDownloadHandler(downloadHandler);
            
            var downloadResult = await Download(
                contentItem,
                contentDownloadHandler,
                cancellationToken);

            return downloadResult.Match(
                onSuccess: () => Result.Success(contentItem),
                onFailure: error => Result.Failure<ContentItem>($"Failed downloading {contentItem}. Error: {error}"));
        }

        /// <summary>
        /// TODO: Do it better
        /// </summary>
        private async Task<IDownloadHandler> GetDownloadHandler(ContentItem contentItem)
        {
            var errors = 0;
            while (errors < 10)
            {
                try
                {
                    return contentItem.ItemType == ItemType.App
                        ? await _steamClient.ContentClient.GetAppDataAsync(
                            SteamConstants.ArmaServerAppId,
                            SteamConstants.ArmaDepotId,
                            os: SteamOs.Windows)
                        : await _steamClient.ContentClient.GetPublishedFileDataAsync(
                            contentItem.Id,
                            os: SteamOs.Windows);
                }
                catch (TaskCanceledException)
                {
                    errors += 1;
                }
                catch (SteamAppAccessTokenDeniedException exception)
                {
                    _logger.LogError(exception, "Workshop item with id {Id} does not exist", contentItem.Id);
                    throw new WorkshopItemNotExistsException(contentItem.Id, exception);
                }
            }

            throw new Exception($"10 errors while attempting to download item {contentItem.Id}");
        }

        private async Task<Result> Download(
            ContentItem contentItem,
            IContentDownloadHandler contentDownloadHandler,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting download of {ContentItem}", contentItem);

            var downloadDirectory = GetModDownloadDirectory(contentItem);
            var downloadTask = contentDownloadHandler.DownloadChangesToFolderAsync(downloadDirectory, cancellationToken);

            return await HandleDownloadTask(
                contentItem.Id,
                contentDownloadHandler,
                downloadTask,
                cancellationToken);
        }

        private string GetModDownloadDirectory(ContentItem contentItem)
            => contentItem.Directory ?? Path.Join(_modsDirectory, contentItem.Id.ToString());

        private async Task<Result> HandleDownloadTask(
            uint itemId,
            IContentDownloadHandler contentDownloadHandler,
            Task downloadTask,
            CancellationToken cancellationToken)
        {
            await Task.Delay(ProgressLogInterval, cancellationToken);
            
            while (!downloadTask.IsCompleted)
            {
                _logger.LogDebug("Item {ItemId} download progress {Progress:00.00}%", itemId, contentDownloadHandler.TotalProgress * 100);
                var delayTask = Task.Delay(ProgressLogInterval, cancellationToken);
                await Task.WhenAny(delayTask, downloadTask);
            }

            await downloadTask;

            if (downloadTask.IsCompletedSuccessfully)
            {
                _logger.LogInformation("Downloaded {ItemId}", itemId);
                return Result.Success();
            }
            else
            {
                _logger.LogInformation("Aborted {ItemId} download", itemId);
                return Result.Failure($"Download of {itemId} was aborted.");
            }
        }

        private Result<IMod> UpdateModData(IMod mod, ContentItem downloadedItem)
        {
            var updatedMod = (IMod) new Mod
            {
                Directory = mod.Directory ?? downloadedItem.Directory,
                CreatedAt = mod.CreatedAt,
                LastUpdatedAt = DateTime.Now,
                Name = mod.Name,
                WorkshopId = mod.WorkshopId,
                Type = mod.Type,
                Source = mod.Source,
                WebId = mod.WebId
            };
            
            _logger.LogTrace("Downloaded mod data: {@Mod}", updatedMod);

            return Result.Success(updatedMod);
        }
    }
}
