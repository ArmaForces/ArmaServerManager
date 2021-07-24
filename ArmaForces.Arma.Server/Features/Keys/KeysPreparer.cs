using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Constants;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.Arma.Server.Features.Keys.Finder;
using ArmaForces.Arma.Server.Features.Keys.IO;
using ArmaForces.Arma.Server.Features.Keys.Models;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Keys
{
    internal class KeysPreparer : IKeysPreparer
    {
        private readonly IModDirectoryFinder _modDirectoryFinder;
        private readonly IKeysFinder _keysFinder;
        private readonly IKeysCopier _keysCopier;
        private readonly ILogger<KeysPreparer> _logger;
        private readonly IFileSystem _fileSystem;

        private readonly string _keysDirectory;
        private readonly string _managerDirectory;
        private readonly string _externalKeysDirectoryPath;

        public KeysPreparer(
            ISettings settings,
            IConfig serverConfig,
            IModDirectoryFinder modDirectoryFinder,
            IKeysFinder keysFinder,
            IKeysCopier keysCopier,
            ILogger<KeysPreparer> logger,
            IFileSystem? fileSystem = null)
        {
            _modDirectoryFinder = modDirectoryFinder;
            _keysFinder = keysFinder;
            _keysCopier = keysCopier;
            _logger = logger;
            _fileSystem = fileSystem ?? new FileSystem();

            _keysDirectory = _fileSystem.Path.Join(settings.ServerDirectory, KeysConstants.KeysDirectoryName);
            _externalKeysDirectoryPath = _fileSystem.Path.Join(serverConfig.DirectoryPath, KeysConstants.ExternalKeysDirectoryName);
            _managerDirectory = settings.ManagerDirectory;
        }

        public Result PrepareKeysForModset(IModset modset) 
            => RemoveOldKeys()
                .Bind(() => CopyNewKeys(modset))
                .Bind(CopyArmaKey);

        private Result RemoveOldKeys()
        {
            var oldKeys = _keysFinder.GetKeysFromDirectory(_keysDirectory);

            _logger.LogDebug("Found {Count} old keys", oldKeys.Count);

            return _keysCopier.DeleteKeys(oldKeys);
        }

        private Result CopyNewKeys(IModset modset)
        {
            var clientLoadableMods = modset.ClientLoadableMods;

            if (clientLoadableMods.IsEmpty())
            {
                _logger.LogInformation("No client loadable mods found in modset {ModsetName}", modset.Name);
            }

            foreach (var mod in clientLoadableMods)
            {
                CopyKeysForMod(mod)
                    .OnFailure(error => _logger.LogWarning(
                        "Copying keys for mod {@Mod} failed with error: {Error}", 
                        mod,
                        error));
            }

            _logger.LogDebug("Keys copying finished for modset {ModsetName}", modset.Name);

            return Result.Success();
        }

        private Result CopyKeysForMod(IMod mod)
        {
            try
            {
                var modBikeys = GetKeysFromMod(mod)
                    .Concat(GetExternalKeys(mod))
                    .ToList();

                _logger.LogTrace(
                    "Found {Count} keys for {@Mod}",
                    modBikeys.Count,
                    mod);

                return _keysCopier.CopyKeys(_keysDirectory, modBikeys);
            }
            // TODO: Remove if does not occur anymore
            catch (ArgumentNullException exception)
            {
                _logger.LogError(
                    exception,
                    "Error copying keys for {@Mod}",
                    mod);
                throw;
            }
        }

        private IEnumerable<BikeyFile> GetKeysFromMod(IMod mod)
        {
            return _keysFinder.GetKeysFromDirectory(mod.Directory);
        }
        
        private IEnumerable<BikeyFile> GetExternalKeys(IMod mod)
        {
            var modDirectoryResult = _modDirectoryFinder.TryFindModDirectory(mod, _externalKeysDirectoryPath);
            
            return modDirectoryResult.IsSuccess
                ? _keysFinder.GetKeysFromDirectory(modDirectoryResult.Value)
                : new List<BikeyFile>();
        }

        /// <summary>
        /// TODO: Do it better. Maybe download it from Depot and store somewhere.
        /// </summary>
        private Result CopyArmaKey()
        {
            var armaKeyPath = _fileSystem.Path.Join(_managerDirectory, KeysConstants.ArmaKey);
            var bikeyFile = new BikeyFile(armaKeyPath);

            return !_fileSystem.File.Exists(bikeyFile.Path)
                ? Result.Failure($"{KeysConstants.ArmaKey} not found in Manager directory.")
                : _keysCopier.CopyKeys(_keysDirectory, bikeyFile.AsList());
        }
    }
}
