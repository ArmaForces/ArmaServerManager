using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using ArmaForces.Arma.Server.Constants;
using ArmaForces.Arma.Server.Features.Keys.Models;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Keys.Finder
{
    internal class KeysFinder : IKeysFinder
    {
        private readonly ILogger<KeysFinder> _logger;
        private readonly IFileSystem _fileSystem;

        public KeysFinder(ILogger<KeysFinder> logger, IFileSystem? fileSystem = null)
        {
            _logger = logger;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public List<BikeyFile> GetKeysFromDirectory(string? directory)
        {
            _logger.LogTrace("Looking for keys in {directory}.", directory);

            var keyFiles = directory is null
                ? new string[0]
                : _fileSystem.Directory.GetFiles(
                    directory,
                    KeysConstants.KeyExtensionSearchPattern,
                    SearchOption.AllDirectories);

            if (keyFiles.Any())
            {
                _logger.LogDebug(
                    "Found {count} keys in {directory}.",
                    keyFiles.Length,
                    directory);
            }
            else
            {
                _logger.LogDebug("No keys found in {directory}.", directory);
            }

            return keyFiles
                .Select(path => new BikeyFile(path))
                .ToList();
        }
    }
}
