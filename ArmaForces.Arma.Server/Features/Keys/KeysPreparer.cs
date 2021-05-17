using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Constants;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Keys
{
    public class KeysPreparer : IKeysPreparer
    {
        private readonly ILogger<KeysPreparer> _logger;
        private readonly IFileSystem _fileSystem;

        private readonly string _keysDirectory;
        private readonly string _managerDirectory;

        public KeysPreparer(
            ISettings settings,
            ILogger<KeysPreparer> logger,
            IFileSystem? fileSystem = null)
        {
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
            var oldKeys = GetKeysFromDirectory(_keysDirectory)
                .ToList();

            _logger.LogDebug("Found {count} old keys.", oldKeys.Count);

            foreach (var oldKey in oldKeys)
            {
                _logger.LogTrace("Removing {keyName} key.", _fileSystem.Path.GetFileName(oldKey));
                _fileSystem.File.Delete(oldKey);
            }

            return Result.Success();
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

                return CopyKeys(modBikeys);
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

        private IEnumerable<string> GetKeysFromMod(IMod mod)
        {
            return GetKeysFromDirectory(mod.Directory);
        }
        
        private IEnumerable<string> GetExternalKeys(IMod mod)
        {
            // TODO: Get directory path of external keys for mod
            return GetKeysFromDirectory(mod.Directory);
        }

        private IEnumerable<string> GetKeysFromDirectory(string? directory)
        {
            _logger.LogTrace("Looking for keys in {directory}.", directory);

            var keyFiles = directory is null
                ? new string[0]
                : _fileSystem.Directory.GetFiles(
                    directory,
                    $"*{KeysConstants.KeyExtension}",
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

            return keyFiles;
        }

        private Result CopyKeys(IReadOnlyCollection<string> modBikeysPaths)
        {
            if (!modBikeysPaths.Any())
            {
                return Result.Failure("No keys found.");
            }

            foreach (var modBikey in modBikeysPaths)
            {
                var keyName = _fileSystem.Path.GetFileName(modBikey);
                var destinationKeyPath = _fileSystem.Path.Join(_keysDirectory, keyName);
                _logger.LogTrace("Copying {keyName}.", keyName);
                if (!_fileSystem.File.Exists(destinationKeyPath))
                {
                    _fileSystem.File.Copy(modBikey, destinationKeyPath);
                }
            }

            return Result.Success();
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
