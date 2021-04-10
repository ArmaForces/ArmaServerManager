using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Steam.Constants;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using BytexDigital.Steam.ContentDelivery.Enumerations;
using BytexDigital.Steam.ContentDelivery.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SteamKit2;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content
{
    public class ContentVerifier : IContentVerifier
    {
        private readonly ISteamClient _steamClient;
        private readonly ILogger<ContentVerifier> _logger;
        private readonly IFileSystem _fileSystem;

        public ContentVerifier(
            ISteamClient steamClient,
            ILogger<ContentVerifier> logger,
            IFileSystem? fileSystem = null)
        {
            _steamClient = steamClient;
            _logger = logger;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public async Task<Result<ContentItem>> ItemIsUpToDate(
            ContentItem contentItem,
            CancellationToken cancellationToken)
        {
            if (contentItem.Directory is null)
            {
                _logger.LogInformation("Item {contentItemId} doesn't have a directory.");

                return Result.Failure<ContentItem>("Item not exists.");
            }

            if (contentItem.ManifestId is null)
            {
                await GetManifestId(contentItem);
            }

            _logger.LogTrace("Downloading Manifest for item {contentItemId}.", contentItem.Id);

            var manifest = await GetManifest(contentItem, cancellationToken);

            var incorrectFiles = manifest.Files
                .SkipWhile(manifestFile => FileIsUpToDate(contentItem.Directory, manifestFile))
                .ToList();

            _logger.LogDebug(
                incorrectFiles.Any()
                    ? "Found incorrect files for item {contentItemId}."
                    : "All files are correct for item {contentItemId}.",
                contentItem.Id);

            return incorrectFiles
                .Any()
                ? Result.Failure<ContentItem>("One or more files are either missing or require update.")
                : Result.Success(contentItem);
        }

        /// <summary>
        /// TODO: Do it better
        /// </summary>
        private async Task GetManifestId(ContentItem contentItem)
        {
            _logger.LogDebug("Downloading ManifestId for item {contentItemId}.", contentItem.Id);

            var errors = 0;

            while (contentItem.ManifestId == null)
            {
                try
                {
                    contentItem.ManifestId =
                        (await _steamClient.ContentClient.GetPublishedFileDetailsAsync(contentItem.Id))
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

        private async Task<Manifest> GetManifest(ContentItem contentItem, CancellationToken cancellationToken)
            => await _steamClient.ContentClient.GetManifestAsync(
                SteamConstants.ArmaAppId,
                SteamConstants.ArmaDepotId,
                contentItem.ManifestId!.Value,
                cancellationToken);

        private bool FileIsUpToDate(string directory, ManifestFile file)
        {
            if (file.Flags == ManifestFileFlag.Directory)
            {
                return _fileSystem.Directory.Exists(Path.Join(directory, file.FileName));
            }

            var filePath = _fileSystem.Path.Combine(directory, file.FileName);

            if (!_fileSystem.File.Exists(filePath)) return false;

            using var fileStream = new FileStream(filePath, FileMode.Open);
            using var bufferedStream = new BufferedStream(fileStream);
            using var sha1 = new SHA1Managed();

            var localFileHash = sha1.ComputeHash(bufferedStream);

            return localFileHash.SequenceEqual(file.FileHash);
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
