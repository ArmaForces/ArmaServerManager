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

        public async Task<Result<ContentItem>> ItemIsUpToDate(ContentItem contentItem, CancellationToken cancellationToken)
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

            while (errors < 10 && contentItem.ManifestId == null)
            {
                try
                {
                    contentItem.ManifestId = (await _steamClient.ContentClient.GetPublishedFileDetailsAsync(contentItem.Id))
                        .hcontent_file;
                }
                catch (TaskCanceledException)
                {
                    errors += 1;
                    _logger.LogTrace("Failed to download ManifestId for item {contentItemId}. Errors = {number}.", contentItem.Id, errors);
                }
            }

            if (errors > 9)
            {
                _logger.LogError("Could not download ManifestId for item {contentItemId}.", contentItem.Id);

                throw new Exception($"10 errors while attempting to download manifest for {contentItem.Id}");
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
    }
}
