using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using BytexDigital.Steam.ContentDelivery.Enumerations;
using BytexDigital.Steam.ContentDelivery.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content
{
    internal class ContentFileVerifier : IContentFileVerifier
    {
        private readonly ILogger<ContentFileVerifier> _logger;
        private readonly IFileSystem _fileSystem;

        public ContentFileVerifier(ILogger<ContentFileVerifier> logger, IFileSystem? fileSystem = null)
        {
            _logger = logger;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public Result<ContentItem> EnsureDirectoryExists(ContentItem contentItem)
        {
            if (contentItem.Directory != null && _fileSystem.Directory.Exists(contentItem.Directory))
            {
                return Result.Success(contentItem);
            }

            _logger.LogInformation("Item {ContentItemId} doesn't have a directory", contentItem.Id);
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

            if (redundantFiles.IsEmpty()) return;
            
            _logger.LogDebug("Found {Count} redundant files in {Directory}", redundantFiles.Count, directory);
                
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
}
