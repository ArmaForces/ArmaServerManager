using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;

namespace Arma.Server.Config {
    public class ServerConfig : IConfig {
        public string BasicCfg { get; protected set; }
        public string ConfigJson { get; protected set; }
        public string DirectoryPath { get; protected set; }
        public string ServerCfg { get; protected set; }

        private readonly ISettings _settings;

        /// <summary>
        /// Class prepares server configuration for given modlist
        /// </summary>
        /// <param name="settings">Server Settings Object</param>
        public ServerConfig(ISettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Handles preparation of all config files.
        /// </summary>
        /// <returns></returns>
        public Result LoadConfig() {
            Console.WriteLine("Loading ServerConfig.");
            
            return SetProperties()
                .Bind(GetOrCreateServerConfigDir)
                .Tap(() => Console.WriteLine("ServerConfig loaded."))
                .OnFailure(e => Console.WriteLine("ServerConfig could not be loaded with {e}.", e));
        }

        private Result SetProperties() {
            DirectoryPath = _settings.ServerConfigDirectory;
            ConfigJson = Path.Join(DirectoryPath, "common.json");
            BasicCfg = Path.Join(DirectoryPath, "basic.cfg");
            ServerCfg = Path.Join(DirectoryPath, "server.cfg");
            return Result.Success();
        }

        /// <summary>
        /// Prepares serverConfig directory with files.
        /// </summary>
        /// <returns>path to serverConfig</returns>
        private Result GetOrCreateServerConfigDir() {
            if (!Directory.Exists(DirectoryPath)) {
                Console.WriteLine($"Config directory {_settings.ServerConfigDirectory} does not exists, creating.");
                Directory.CreateDirectory(DirectoryPath);
            }

            // Prepare files
            var filesList = new List<string>() {"basic.cfg", "server.cfg", "common.Arma3Profile", "common.json"};
            foreach (var fileName in filesList) {
                var destFileName = Path.Join(DirectoryPath, fileName);
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
                    cfgFile = ConfigReplacer.ReplaceValue(cfgFile, key, stringValue);
                } else {
                    cfgFile = ConfigReplacer.ReplaceValue(cfgFile, key, config[key]);
                }
            }

            return cfgFile;
        }
    }
}