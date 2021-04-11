using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using BytexDigital.Steam.ContentDelivery.Enumerations;
using BytexDigital.Steam.ContentDelivery.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content
{
    internal interface IContentFileVerifier
    {
        Result<ContentItem> EnsureDirectoryExists(ContentItem contentItem);

        void RemoveRedundantFiles(string directory, Manifest manifest);

        bool FileIsUpToDate(string directory, ManifestFile file);
    }

    internal class ContentFileVerifier : IContentFileVerifier
    {
        private readonly ILogger<ContentFileVerifier> _logger;
        private readonly IFileSystem _fileSystem;

        public ContentFileVerifier(ILogger<ContentFileVerifier> logger, IFileSystem fileSystem)
        {
            _logger = logger;
            _fileSystem = fileSystem;
        }

        public Result<ContentItem> EnsureDirectoryExists(ContentItem contentItem)
        {
            if (contentItem.Directory != null && _fileSystem.Directory.Exists(contentItem.Directory))
            {
                return Result.Success(contentItem);
            }

            _logger.LogInformation("Item {contentItemId} doesn't have a directory.");
            return Result.Failure<ContentItem>("Item not exists.");
        }

        public void RemoveRedundantFiles(string directory, Manifest manifest)
        {
            var filesAndDirectories = manifest.Files
                .Select(x => x.FileName)
                .ToList();

            RemoveRedundantFiles(directory, filesAndDirectories);
        }

        public void RemoveRedundantFiles(string directory, IReadOnlyCollection<string> expectedFilesAndDirectories)
        {
            var expectedFiles = expectedFilesAndDirectories
                .Select(x => Path.Join(directory, x))
                .ToList();

            var redundantFiles = _fileSystem.Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                .Where(x => !expectedFiles.Contains(x))
                .ToList();

            foreach (var file in redundantFiles)
            {
                _fileSystem.File.Delete(file);
            }
        }

        public bool FileIsUpToDate(string directory, ManifestFile file)
        {
            if (file.Flags == ManifestFileFlag.Directory)
            {
                return _fileSystem.Directory.Exists(Path.Join(directory, file.FileName));
            }

            var filePath = _fileSystem.Path.Combine(directory, file.FileName);

            if (!_fileSystem.File.Exists(filePath)) return false;

            using var fileStream = _fileSystem.FileStream.Create(filePath, FileMode.Open);
            using var bufferedStream = new BufferedStream(fileStream);
            using var sha1 = new SHA1Managed();

            var localFileHash = sha1.ComputeHash(bufferedStream);

            return localFileHash.SequenceEqual(file.FileHash);
        }
    }

    internal class ContentVerifier : IContentVerifier
    {
        private readonly IManifestDownloader _manifestDownloader;
        private readonly ILogger<ContentVerifier> _logger;
        private readonly IContentFileVerifier _contentFileVerifier;

        public ContentVerifier(
            IManifestDownloader manifestDownloader,
            IContentFileVerifier contentFileVerifier,
            ILogger<ContentVerifier> logger)
        {
            _contentFileVerifier = contentFileVerifier;
            _manifestDownloader = manifestDownloader;
            _logger = logger;
        }

        public async Task<Result<ContentItem>> ItemIsUpToDate(
            ContentItem contentItem,
            CancellationToken cancellationToken)
        {
            await _contentFileVerifier.EnsureDirectoryExists(contentItem)
                .Bind(x => Result.Success(_manifestDownloader.GetManifest(x, cancellationToken)))
                .Tap(x => x);
            
            var manifest = await _manifestDownloader.GetManifest(contentItem, cancellationToken);

            var incorrectFiles = manifest.Files
                .SkipWhile(manifestFile => _contentFileVerifier.FileIsUpToDate(contentItem.Directory, manifestFile))
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
    }
}
