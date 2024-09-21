using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using ArmaForces.Arma.Server.Common.Errors;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Config {
    public class ServerConfig : IConfig {
        public string BasicCfg { get; protected set; }
        public string ConfigJson { get; protected set; }
        public string DirectoryPath { get; protected set; }
        public string ServerCfg { get; protected set; }

        private readonly ISettings _settings;
        private readonly ILogger<ServerConfig> _logger;
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// Class prepares server configuration for given modset
        /// </summary>
        /// <param name="settings">Server Settings Object</param>
        /// TODO: Make factory, just as for ModsetConfig
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public ServerConfig(
        ISettings settings,
        ILogger<ServerConfig> logger,
        IFileSystem? fileSystem = null)
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
            _settings = settings;
            _logger = logger;
            _fileSystem = fileSystem ?? new FileSystem();
            SetProperties();
        }

        /// <summary>
        /// Handles preparation of all config files.
        /// </summary>
        /// <returns></returns>
        public UnitResult<IError> CopyConfigFiles() {
            _logger.LogDebug("Loading ServerConfig");
            
            return GetOrCreateServerConfigDir()
                .Tap(() => _logger.LogInformation("ServerConfig files copied"))
                .TapError(error => _logger.LogError("ServerConfig files could not be copied: {Error}", error));
        }

        private void SetProperties() {
            DirectoryPath = _settings.ServerConfigDirectory!;
            ConfigJson = Path.Join(DirectoryPath, "common.json");
            BasicCfg = Path.Join(DirectoryPath, "basic.cfg");
            ServerCfg = Path.Join(DirectoryPath, "server.cfg");
        }

        /// <summary>
        /// Prepares serverConfig directory with files.
        /// </summary>
        /// <returns>path to serverConfig</returns>
        private UnitResult<IError> GetOrCreateServerConfigDir() {
            if (!_fileSystem.Directory.Exists(DirectoryPath)) {
                _logger.LogDebug("Config directory {Directory} does not exists, creating", _settings.ServerConfigDirectory);
                _fileSystem.Directory.CreateDirectory(DirectoryPath);
            }

            // Prepare files
            var filesList = new List<string>() {"basic.cfg", "server.cfg", "common.Arma3Profile", "common.json"};
            foreach (var fileName in filesList) {
                var destFileName = Path.Join(DirectoryPath, fileName);
                if (_fileSystem.File.Exists(destFileName)) continue;
                _logger.LogTrace("{FileName} not found, copying", fileName);

                var exampleFilePath = Path.Join(_fileSystem.Directory.GetCurrentDirectory(), $"example_{fileName}");
                _fileSystem.File.Copy(exampleFilePath, destFileName);
            }

            return UnitResult.Success<IError>();
        }
    }
}