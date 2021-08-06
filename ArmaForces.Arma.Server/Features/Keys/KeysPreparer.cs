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
            var oldKeys = _keysFinder.GetKeysFromDirectory(_keysDirectory)
                .Select(path => new BikeyFile(path))
                .ToList();

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

            var modBikeysList = new List<ModBikeys>();
            foreach (var mod in clientLoadableMods)
            {
                CopyKeysForMod(mod)
                    .Tap(modBikeys => modBikeysList.Add(modBikeys))
                    .OnFailure(error => LogKeysCopyError(mod, error))
                    .OnFailure(_ => modBikeysList.Add(new ModBikeys(mod)));
            }

            _logger.LogInformation(
                "Copied {Count} keys for modset {ModsetName}", 
                modBikeysList.Count(x => x.BikeyFiles.Any()),
                modset.Name);

            LogKeysNotFound(modBikeysList);

            return Result.Success();
        }

        private Result<ModBikeys> CopyKeysForMod(IMod mod)
        {
            try
            {
                var keysForMod = GetAllKeysForMod(mod);

                _logger.LogTrace(
                    "Found {Count} keys for {@Mod}",
                    keysForMod.BikeyFiles.Count,
                    mod);

                return _keysCopier.CopyKeys(_keysDirectory, keysForMod.BikeyFiles)
                    .Bind(() => Result.Success(keysForMod));
            }
            // TODO: Remove if does not occur anymore
            catch (ArgumentNullException exception)
            {
                _logger.LogError(
                    exception,
                    "Error copying keys for {Mod}",
                    mod.ToShortString());
                throw;
            }
        }

        private ModBikeys GetAllKeysForMod(IMod mod)
        {
            var bikeyFiles = GetKeysFromMod(mod)
                .Concat(GetExternalKeys(mod))
                .ToList();

            return new ModBikeys(mod, bikeyFiles);
        }

        private List<BikeyFile> GetKeysFromMod(IMod mod)
        {
            _logger.LogDebug("Looking for standard keys for {Mod}", mod.ToShortString());
            
            return _keysFinder.GetKeysFromDirectory(mod.Directory)
                .Select(path => new BikeyFile(path, mod.ToShortString()))
                .ToList();
        }

        private List<BikeyFile> GetExternalKeys(IMod mod)
        {
            var result = _modDirectoryFinder.TryFindModDirectory(mod, _externalKeysDirectoryPath)
                .Tap(_ => _logger.LogDebug("Looking for external keys for {Mod}", mod.ToShortString()));

            return result.IsSuccess
                ? _keysFinder.GetKeysFromDirectory(result.Value)
                    .Select(path => new BikeyFile(path, mod.ToShortString()))
                    .ToList()
                : new List<BikeyFile>();
        }

        /// <summary>
        /// TODO: Do it better. Maybe download it from Depot and store somewhere.
        /// </summary>
        private Result CopyArmaKey()
        {
            var armaKeyPath = _fileSystem.Path.Join(_managerDirectory, KeysConstants.ArmaKey);
            var bikeyFile = new BikeyFile(armaKeyPath, ArmaConstants.GameName);

            return !_fileSystem.File.Exists(bikeyFile.Path)
                ? Result.Failure($"{KeysConstants.ArmaKey} not found in Manager directory.")
                : _keysCopier.CopyKeys(_keysDirectory, bikeyFile.AsList());
        }

        private void LogKeysNotFound(IReadOnlyCollection<ModBikeys> modBikeysList)
        {
            var modsWithNoKeys = modBikeysList
                .Where(x => x.BikeyFiles.IsEmpty())
                .Select(x => x.Mod.ToShortString())
                .ToList();

            if (modsWithNoKeys.Any())
            {
                _logger.LogWarning("No keys found for {Count} mods", modsWithNoKeys.Count);
                _logger.LogDebug("No keys found for {@Mods}", modsWithNoKeys);
            }
        }

        private void LogKeysCopyError(IMod mod, string error)
        {
            _logger.LogWarning(
                "Copying keys for mod {Mod} failed with error: {Error}",
                mod.ToShortString(),
                error);
            
            _logger.LogTrace("Copying keys failed for mod {@Mod}", mod);
        }

        private readonly struct ModBikeys
        {
            public IReadOnlyCollection<BikeyFile> BikeyFiles { get; }
            public IMod Mod { get; }

            public ModBikeys(IMod mod) : this(mod, new List<BikeyFile>()) {}

            public ModBikeys(IMod mod, IReadOnlyCollection<BikeyFile> bikeyFiles)
            {
                BikeyFiles = bikeyFiles;
                Mod = mod;
            }
        }
    }
}
