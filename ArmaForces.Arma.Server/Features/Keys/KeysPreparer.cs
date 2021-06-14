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
        private readonly string _modsetConfigDirectoryPath;

        public KeysPreparer(
            ISettings settings,
            IModsetConfig modsetConfig,
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

            _keysDirectory = _fileSystem.Path.Join(settings.ServerDirectory, "Keys");
            _modsetConfigDirectoryPath = modsetConfig.DirectoryPath;
            _managerDirectory = settings.ManagerDirectory;
        }

        public Result PrepareKeysForModset(IModset modset) 
            => RemoveOldKeys()
                .Bind(() => CopyNewKeys(modset))
                .Bind(CopyArmaKey);

        private Result RemoveOldKeys()
        {
            var oldKeys = _keysFinder.GetKeysFromDirectory(_keysDirectory)
                .ToList();

            _logger.LogDebug("Found {count} old keys.", oldKeys.Count);

            return _keysCopier.DeleteKeys(oldKeys);
        }

        private Result CopyNewKeys(IModset modset)
        {
            var clientLoadableMods = modset.ClientLoadableMods;

            if (!clientLoadableMods.Any())
            {
                _logger.LogInformation("No client loadable mods found in modset {modsetName}.", modset.Name);
            }

            foreach (var mod in clientLoadableMods)
            {
                var copyKeysForMod = CopyKeysForMod(mod);

                if (copyKeysForMod.IsFailure)
                {
                    _logger.LogWarning(
                        "Copying keys for mod {mod} failed with error: {error}", 
                        mod,
                        copyKeysForMod.Error);
                }
            }

            _logger.LogDebug("Keys copying finished for modset {modsetName}.", modset.Name);

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
                    "Found {count} keys for {mod}",
                    modBikeys.Count,
                    mod.ToShortString());

                return _keysCopier.CopyKeys(_keysDirectory, modBikeys);
            }
            // TODO: Remove if does not occur anymore
            catch (ArgumentNullException exception)
            {
                _logger.LogError(
                    exception,
                    "Error copying keys for {mod}.",
                    mod.ToShortString());
                throw;
            }
        }

        private IEnumerable<BikeyFile> GetKeysFromMod(IMod mod)
        {
            return _keysFinder.GetKeysFromDirectory(mod.Directory);
        }
        
        private IEnumerable<BikeyFile> GetExternalKeys(IMod mod)
        {
            // TODO: Use IModDirectoryFinder to support multiple directory name patterns
            var externalKeysDirectory = _fileSystem.Path.Join(_modsetConfigDirectoryPath, mod.Name);
            return _keysFinder.GetKeysFromDirectory(externalKeysDirectory);
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
