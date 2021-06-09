using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using ArmaForces.Arma.Server.Features.Keys.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Keys.IO
{
    public class KeysCopier : IKeysCopier
    {
        private readonly ILogger<KeysCopier> _logger;
        private readonly IFileSystem _fileSystem;

        public KeysCopier(ILogger<KeysCopier> logger, IFileSystem? fileSystem = null)
        {
            _logger = logger;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public Result DeleteKeys(IReadOnlyCollection<BikeyFile> bikeyFiles)
        {
            foreach (var bikeyFile in bikeyFiles)
            {
                _logger.LogTrace("Removing {keyName} key.", _fileSystem.Path.GetFileName(bikeyFile.Path));
                
                _fileSystem.File.Delete(bikeyFile.Path);
            }

            return Result.Success();
        }

        public Result CopyKeys(string targetDirectory, IReadOnlyCollection<BikeyFile> bikeyFiles)
        {
            if (!bikeyFiles.Any())
            {
                return Result.Failure("No keys found.");
            }

            foreach (var modBikey in bikeyFiles)
            {
                var keyName = _fileSystem.Path.GetFileName(modBikey.Path);
                var destinationKeyPath = _fileSystem.Path.Join(targetDirectory, keyName);
                
                _logger.LogTrace("Copying {keyName}.", keyName);
                
                if (!_fileSystem.File.Exists(destinationKeyPath))
                {
                    _fileSystem.File.Copy(modBikey.Path, destinationKeyPath);
                }
            }

            return Result.Success();
        }
    }
}
