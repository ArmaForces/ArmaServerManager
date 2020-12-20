using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.ArmaServerManager.Constants;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using ArmaForces.ArmaServerManager.Features.Steam.Content.Exceptions;
using BytexDigital.Steam.ContentDelivery.Exceptions;
using BytexDigital.Steam.ContentDelivery.Models.Downloading;
using BytexDigital.Steam.Core.Enumerations;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content
{
    /// <inheritdoc />
    public class ContentDownloader : IContentDownloader
    {
        private readonly string _modsDirectory;
        private readonly ISteamClient _steamClient;

        public ContentDownloader(ISettings settings) : this(new SteamClient(settings), settings.ModsDirectory)
        {
        }

        /// <inheritdoc cref="ContentDownloader" />
        /// <param name="steamClient">Client used for connection.</param>
        /// <param name="modsDirectory">Directory where mods should be stored.</param>
        public ContentDownloader(
            ISteamClient steamClient,
            string modsDirectory)
        {
            _steamClient = steamClient;
            _modsDirectory = modsDirectory;
        }

        public async Task<List<Result<IMod>>> DownloadOrUpdateMods(
            IEnumerable<IMod> mods,
            CancellationToken cancellationToken)
        {
            await _steamClient.EnsureConnected(cancellationToken);
            var workshopMods = mods.Where(x => x.Source == ModSource.SteamWorkshop);

            var results = new List<Result<IMod>>();
            foreach (var mod in workshopMods)
            {
                if (cancellationToken.IsCancellationRequested) CancelDownload();
                var item = mod.AsContentItem();
                var result = await DownloadOrUpdate(item, cancellationToken);

                if (result.IsSuccess)
                {
                    var downloadedItem = result.Value;
                    var updatedMod = (IMod) new Mod
                    {
                        Directory = mod.Directory ?? downloadedItem.Directory,
                        CreatedAt = mod.CreatedAt,
                        LastUpdatedAt = DateTime.Now,
                        Name = mod.Name,
                        WorkshopId = mod.WorkshopId,
                        Type = mod.Type,
                        ManifestId = downloadedItem.ManifestId,
                        Source = mod.Source,
                        WebId = mod.WebId
                    };
                    results.Add(Result.Success(updatedMod));
                }
                else
                {
                    results.Add(Result.Failure<IMod>(result.Error));
                }
            }

            return results;
        }

        public async Task<List<Result<ContentItem>>> DownloadOrUpdate(
            IEnumerable<ContentItem> items,
            CancellationToken cancellationToken)
        {
            await _steamClient.EnsureConnected(cancellationToken);

            var results = new List<Result<ContentItem>>();
            foreach (var item in items)
            {
                if (cancellationToken.IsCancellationRequested) CancelDownload();
                results.Add(await DownloadOrUpdate(item, cancellationToken));
            }

            return results;
        }

        public async Task<Result<ContentItem>> DownloadOrUpdate(
            ContentItem contentItem,
            CancellationToken cancellationToken)
            => await Download(contentItem, cancellationToken);
        
        public static ContentDownloader CreateContentDownloader(IServiceProvider serviceProvider)
        {
            var modsDirectory = serviceProvider.GetService<ISettings>().ModsDirectory;
            return new ContentDownloader(serviceProvider.GetService<ISteamClient>(), modsDirectory);
        }

        /// <summary>
        ///     Safely cancels download process.
        /// </summary>
        private void CancelDownload() => throw new OperationCanceledException();

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
            Console.WriteLine($"Starting download of {contentItem}");

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
            
            while (!downloadTask.IsCompleted)
            {
                var delayTask = Task.Delay(1000, cancellationToken);
                var completedTask = await Task.WhenAny(delayTask, downloadTask);
                Console.WriteLine($"Progress {contentDownloadHandler.TotalProgress * 100:00.00}%");
            }

            await downloadTask;

            Console.WriteLine(
                downloadTask.IsCompletedSuccessfully
                    ? $"Downloaded {itemId}."
                    : $"Aborted {itemId} download.");

            return Result.Success();
        }
    }
}
