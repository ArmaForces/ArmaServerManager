using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Constants;
using ArmaForces.Arma.Server.Features.Keys.Finder;
using ArmaForces.Arma.Server.Features.Keys.IO;
using ArmaForces.Arma.Server.Features.Keys.Models;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Keys
{
    public class KeysPreparer : IKeysPreparer
    {
        private readonly IKeysFinder _keysFinder;
        private readonly IKeysCopier _keysCopier;
        private readonly ILogger<KeysPreparer> _logger;
        private readonly IFileSystem _fileSystem;

        private readonly string _keysDirectory;
        private readonly string _managerDirectory;

        public KeysPreparer(
            ISettings settings,
            IKeysFinder keysFinder,
            IKeysCopier keysCopier,
            ILogger<KeysPreparer> logger,
            IFileSystem? fileSystem = null)
        {
            _keysFinder = keysFinder;
            _keysCopier = keysCopier;
            _logger = logger;
            _fileSystem = fileSystem ?? new FileSystem();

            _keysDirectory = _fileSystem.Path.Join(settings.ServerDirectory, "Keys");
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
            // TODO: Get directory path of external keys for mod
            return _keysFinder.GetKeysFromDirectory(mod.Directory);
        }

        /// <summary>
        /// TODO: Do it better. Maybe download it from Depot and store somewhere.
        /// </summary>
        private Result CopyArmaKey()
        {
            var armaKeyPath = _fileSystem.Path.Join(_managerDirectory, KeysConstants.ArmaKey);

            if (!_fileSystem.File.Exists(armaKeyPath))
            {
                return Result.Failure($"{KeysConstants.ArmaKey} not found in Manager directory.");
            }

            var targetFileName = _fileSystem.Path.Join(_keysDirectory, KeysConstants.ArmaKey);

            _fileSystem.File.Copy(armaKeyPath, targetFileName);
            
            return Result.Success();
        }
    }
}
