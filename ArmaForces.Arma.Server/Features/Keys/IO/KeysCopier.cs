﻿using System.Collections.Generic;
using System.IO.Abstractions;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.Arma.Server.Features.Keys.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Keys.IO
{
    internal class KeysCopier : IKeysCopier
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
                _logger.LogDebug("Removing {KeyName}", bikeyFile.FileName);
                
                _fileSystem.File.Delete(bikeyFile.Path);
            }

            return Result.Success();
        }

        public Result CopyKeys(string targetDirectory, IReadOnlyCollection<BikeyFile> bikeyFiles)
        {
            if (bikeyFiles.IsEmpty())
            {
                return Result.Failure("No keys found.");
            }

            foreach (var modBikey in bikeyFiles)
            {
                var destinationKeyPath = _fileSystem.Path.Join(targetDirectory, modBikey.FileName);
                
                _logger.LogDebug("Copying {KeyName}", modBikey.FileName);
                
                if (!_fileSystem.File.Exists(destinationKeyPath))
                {
                    _fileSystem.File.Copy(modBikey.Path, destinationKeyPath);
                }
            }

            return Result.Success();
        }
    }
}
