using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using BytexDigital.Steam.ContentDelivery.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content
{
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
            var incorrectFilesResult = await VerifyAllFiles(contentItem, cancellationToken);

            return incorrectFilesResult
                .TapError(() => LogFailedToVerifyItem(contentItem))
                .OnFailureCompensate(() => new List<ManifestFile>())
                .Bind(x => x.Any()
                    ? Result.Failure<ContentItem>("One or more files are either missing or require update.")
                    : Result.Success(contentItem));
        }

        private async Task<Result<List<ManifestFile>>> VerifyAllFiles(ContentItem contentItem, CancellationToken cancellationToken)
        {
            return await _contentFileVerifier.EnsureDirectoryExists(contentItem)
                .Bind(x => GetManifest(x, cancellationToken))
                // TODO: Move redundant files removing out of verification, returning redundant files should be separate from removing them.
                .Tap(_ => _logger.LogTrace("Searching redundant files for {Item}", contentItem.ToString()))
                .Tap(x => _contentFileVerifier.RemoveRedundantFiles(contentItem.Directory!, x))
                .Tap(_ => _logger.LogTrace("Searching outdated files for {Item}", contentItem.ToString()))
                .Bind(manifest => IsAnyFileOutdated(contentItem, manifest, cancellationToken));
        }

        private Result<List<ManifestFile>> IsAnyFileOutdated(
            ContentItem contentItem,
            Manifest manifest,
            CancellationToken cancellationToken)
            => manifest.Files
                .SkipWhile(manifestFile => cancellationToken.IsCancellationRequested ||
                                           _contentFileVerifier.FileIsUpToDate(contentItem.Directory!, manifestFile))
                .ToList();

        private async Task<Result<Manifest>> GetManifest(ContentItem contentItem, CancellationToken cancellationToken)
            => await _manifestDownloader.GetManifest(contentItem, cancellationToken);

        private void LogFailedToVerifyItem(ContentItem contentItem)
            => _logger.LogWarning("Failed to verify whether content item {ContentItemId} is up to date. " +
                                  "If the issue persists, double check whether the item is still on Workshop", contentItem.Id);
    }
}
