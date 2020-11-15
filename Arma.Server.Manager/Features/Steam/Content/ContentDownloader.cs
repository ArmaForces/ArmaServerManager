using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Config;
using Arma.Server.Manager.Constants;
using Arma.Server.Manager.Features.Steam.Content.DTOs;
using BytexDigital.Steam.ContentDelivery.Models;
using BytexDigital.Steam.ContentDelivery.Models.Downloading;
using BytexDigital.Steam.Core.Enumerations;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;

namespace Arma.Server.Manager.Features.Steam.Content
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
        /// <param name="cancellationToken"></param>
        /// <param name="itemId">Id of item to download.</param>
        /// <param name="itemType">Type of item, App or Mod.</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        private async Task<Result<ContentItem>> Download(ContentItem contentItem, CancellationToken cancellationToken)
        {
            if (contentItem.ItemType == ItemType.App)
                throw new NotImplementedException("Downloading Arma 3 Server is not supported yet.");

            var downloadHandler = await GetDownloadHandler(contentItem);

            var contentDownloadHandler = new ContentDownloadHandler(downloadHandler);

            var downloadResult = await Download(
                contentItem.Id,
                contentDownloadHandler,
                cancellationToken);

            return downloadResult.Match(
                onSuccess: () => Result.Success(contentItem),
                onFailure: error => Result.Failure<ContentItem>($"Failed downloading {contentItem}. Error: {error}"));
        }
        
        private async Task<IDownloadHandler> GetDownloadHandler(ContentItem contentItem) 
            => contentItem.ItemType == ItemType.App
                ? await _steamClient.ContentClient.GetAppDataAsync(
                    SteamConstants.ArmaAppId,
                    SteamConstants.ArmaDepotId,
                    os: SteamOs.Windows)
                : await _steamClient.ContentClient.GetPublishedFileDataAsync(contentItem.Id, os: SteamOs.Windows);

        private async Task<Result> Download(
            uint itemId,
            IContentDownloadHandler contentDownloadHandler,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"Starting download of {itemId}");

            var downloadDirectory = GetModDownloadDirectory(_modsDirectory, itemId);
            var downloadTask = contentDownloadHandler.DownloadChangesToFolderAsync(downloadDirectory, cancellationToken);

            return await HandleDownloadTask(
                itemId,
                contentDownloadHandler,
                downloadTask,
                cancellationToken);
        }

        private static string GetModDownloadDirectory(string modsDirectory, uint itemId)
            => Path.Join(modsDirectory, itemId.ToString());

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
