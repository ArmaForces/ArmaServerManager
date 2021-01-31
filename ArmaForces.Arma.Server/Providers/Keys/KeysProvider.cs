using System.IO;
using System.IO.Abstractions;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Constants;
using ArmaForces.Arma.Server.Features.Modsets;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Providers.Keys
{
    public class KeysProvider : IKeysProvider
    {
        private readonly ILogger<KeysProvider> _logger;
        private readonly IFileSystem _fileSystem;

        private readonly string _keysDirectory;
        private readonly string _managerDirectory;

        public KeysProvider(
            ISettings settings,
            ILogger<KeysProvider> logger,
            IFileSystem fileSystem = null)
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
            var oldKeys = _fileSystem.Directory.GetFiles(_keysDirectory);

            _logger.LogDebug("Found {count} old keys.", oldKeys.Length);

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

            foreach (var mod in clientLoadableMods)
            {
                var directory = mod.Directory;

                var modBikeys = _fileSystem.Directory.GetFiles(
                    directory,
                    $"*{KeysConstants.KeyExtension}",
                    SearchOption.AllDirectories);

                _logger.LogTrace("Found {count} keys for {mod}", modBikeys.Length, mod.ToShortString());

                foreach (var modBikey in modBikeys)
                {
                    var keyName = _fileSystem.Path.GetFileName(modBikey);
                    var destinationKeyPath = _fileSystem.Path.Join(_keysDirectory, keyName);
                    _logger.LogTrace("Copying {keyName}.", keyName);
                    if (!_fileSystem.File.Exists(destinationKeyPath))
                    {
                        _fileSystem.File.Copy(modBikey, destinationKeyPath);
                    }
                }
            }

            _logger.LogDebug("New keys copied.");

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
