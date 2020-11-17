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
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Features.Steam.Content
{
    public class ContentVerifier : IContentVerifier
    {
        private readonly ISteamClient _steamClient;
        private readonly IFileSystem _fileSystem;

        public ContentVerifier(ISettings settings, IFileSystem fileSystem = null) : this(
            new SteamClient(settings),
            fileSystem)
        {
        }

        public ContentVerifier(ISteamClient steamClient, IFileSystem fileSystem = null)
        {
            _steamClient = steamClient;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public async Task<Result> ItemIsUpToDate(ContentItem contentItem, CancellationToken cancellationToken)
        {
            if (contentItem.Directory is null) return Result.Failure("Item not exists.");
            
            contentItem.ManifestId ??=
                (await _steamClient.ContentClient.GetPublishedFileDetailsAsync(contentItem.Id)).hcontent_file;

            var manifest = await GetManifest(contentItem, cancellationToken);

            return manifest.Files
                .SkipWhile(manifestFile => FileIsUpToDate(contentItem.Directory, manifestFile))
                .Any()
                ? Result.Failure("One or more files are either missing or require update.")
                : Result.Success();
        }

        private async Task<Manifest> GetManifest(ContentItem contentItem, CancellationToken cancellationToken)
            => await _steamClient.ContentClient.GetManifestAsync(
                SteamConstants.ArmaAppId,
                SteamConstants.ArmaDepotId,
                contentItem.ManifestId!.Value,
                cancellationToken);

        private bool FileIsUpToDate(string directory, ManifestFile file)
        {
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
