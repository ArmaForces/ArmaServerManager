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
                    .Bind(x => x.Any()
                        ? Result.Failure<ContentItem>("One or more files are either missing or require update.")
                        : Result.Success(contentItem));
        }

        private async Task<Result<List<ManifestFile>>> VerifyAllFiles(ContentItem contentItem, CancellationToken cancellationToken)
        {
            return await _contentFileVerifier.EnsureDirectoryExists(contentItem)
                .Bind(x => GetManifest(x, cancellationToken))
                // TODO: Move redundant files removing out of verification, returning redundant files should be separate from removing them.
                .Tap(x => _contentFileVerifier.RemoveRedundantFiles(contentItem.Directory!, x))
                .Bind(manifest => GetIncorrectFiles(contentItem, manifest));
        }

        private Result<List<ManifestFile>> GetIncorrectFiles(ContentItem contentItem, Manifest manifest)
        {
            return manifest.Files
                .SkipWhile(manifestFile => _contentFileVerifier.FileIsUpToDate(contentItem.Directory!, manifestFile))
                .ToList();
        }

        private async Task<Result<Manifest>> GetManifest(ContentItem contentItem, CancellationToken cancellationToken)
        {
            var manifest = await _manifestDownloader.GetManifest(contentItem, cancellationToken);
            return Result.Success(manifest);
        }
    }
}
