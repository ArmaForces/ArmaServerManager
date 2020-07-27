using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Linq;
using CSharpFunctionalExtensions;

namespace ArmaServerManager {
    public class ServerConfig: IConfig {
        private readonly ISettings _settings;
        private readonly string _serverConfigDirPath;
        private readonly string _modsetConfigDirPath;

        /// <summary>
        /// Class prepares server configuration for given modset
        /// </summary>
        /// <param name="settings">Server Settings Object</param>
        public ServerConfig(ISettings settings) {
            _settings = settings;

            _serverConfigDirPath = CreateServerConfigDirPath();
        }

        public string GetConfigDir()
        {
            return _serverConfigDirPath;
        }

        private string CreateServerConfigDirPath()
        {
            var serverPath = _settings.GetServerPath();
            var serverConfigDirName = _settings.GetSettingsValue("serverConfigDirName").ToString();
            return Path.Join(serverPath, serverConfigDirName);
        }
        
        /// <summary>
        /// Handles preparation of all config files.
        /// </summary>
        /// <returns></returns>
        public Result LoadConfig() {
            Console.WriteLine("Loading ServerConfig.");
            return GetOrCreateServerConfigDir()
                .Tap(() => Console.WriteLine("ServerConfig loaded."))
                .OnFailure(e => Console.WriteLine("ServerConfig could not be loaded with {e}.", e));
        }

        /// <summary>
        /// Prepares serverConfig directory with files.
        /// </summary>
        /// <returns>path to serverConfig</returns>
        private Result GetOrCreateServerConfigDir() {
            var serverPath = _settings.GetServerPath();
            var serverConfigDirName = _settings.GetSettingsValue("serverConfigDirName").ToString();
            var serverConfigDir = Path.Join(serverPath, serverConfigDirName);
            if (!Directory.Exists(serverConfigDir)) {
                Console.WriteLine($"Config directory {serverConfigDirName} does not exists, creating.");
                Directory.CreateDirectory(serverConfigDir);
            }

            // Prepare files
            var filesList = new List<string>() {"basic.cfg", "server.cfg", "common.Arma3Profile", "common.json"};
            foreach (var fileName in filesList) {
                var destFileName = Path.Join(serverConfigDir, fileName);
                if (File.Exists(destFileName)) continue;
                Console.WriteLine($"{fileName} not found, copying.");
                File.Copy(Path.Join(Directory.GetCurrentDirectory(), $"example_{fileName}"), destFileName);
            }

            return Result.Success();
        }

        /// <summary>
        /// In given cfgFile string replaces values for keys present in given config. Returns filled string.
        /// </summary>
        public static string FillCfg(string cfgFile, IConfigurationSection config) {
            foreach (var section in config.GetChildren()) {
                var key = section.Key;
                var value = config.GetSection(key).GetChildren().ToList();
                // Replace default value in cfg with value from loaded configs
                if (value.Count != 0) {
                    // If value is array, it needs changing to string
                    var stringValue = String.Join(", ", value.Select(p => p.Value.ToString()));
                    cfgFile = ServerConfigReplacer.ReplaceValue(cfgFile, key, stringValue);
                } else {
                    cfgFile = ServerConfigReplacer.ReplaceValue(cfgFile, key, config[key]);
                }
            }

            return cfgFile;
        }
    }
}