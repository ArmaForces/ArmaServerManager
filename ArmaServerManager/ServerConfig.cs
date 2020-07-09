using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace ArmaServerManager {
    public class ServerConfig {
        private readonly Settings _settings;
        private readonly Modset _modset;
        private string _serverConfigDir;
        private string _modsetConfigDir;

        /// <summary>
        /// Class prepares server configuration for given modset
        /// </summary>
        /// <param name="settings">Server Settings Object</param>
        /// <param name="modset">Modset object</param>
        public ServerConfig(Settings settings, Modset modset) {
            Console.WriteLine("Loading ServerConfig.");
            _settings = settings;
            _modset = modset;
            // Load config directory and create if it not exists
            PrepareCommonFiles();
            PrepareModsetConfig();

            Console.WriteLine("ServerConfig loaded.");
        }

        /// <summary>
        /// Prepares all config directories for server and modset.
        /// </summary>
        public void PrepareCommonFiles() {
            _serverConfigDir = GetOrCreateServerConfigDir();
            _modsetConfigDir = GetOrCreateModsetConfigDir();
            CreateServerConfigIfDontExists();
            CreateModsetConfigIfDontExists();
        }

        /// <summary>
        /// Prepares serverConfig directory.
        /// </summary>
        /// <returns>path to serverConfig</returns>
        private string GetOrCreateServerConfigDir() {
            var serverPath = _settings.GetServerPath();
            var serverConfigDirName = _settings.GetSettingsValue("serverConfigDirName").ToString();
            var serverConfigDir = Path.Join(serverPath, serverConfigDirName);
            if (!Directory.Exists(serverConfigDir)) {
                Console.WriteLine($"Config directory {serverConfigDirName} does not exists, creating.");
                Directory.CreateDirectory(serverConfigDir);
            }

            return serverConfigDir;
        }

        /// <summary>
        /// Prepares config directory and files for current modset
        /// </summary>
        /// <returns>path to modsetConfig</returns>
        private string GetOrCreateModsetConfigDir() {
            // Get modset config directory based on serverConfig
            var modsetConfigDir = Path.Join(_serverConfigDir, "modsetConfigs", _modset.GetName());

            // Check for directory if present
            if (!Directory.Exists(modsetConfigDir)) {
                Directory.CreateDirectory(modsetConfigDir);
            }
            
            return modsetConfigDir;
        }

        /// <summary>
        /// Checks if default configuration files exists and copies them from example files if they don't exist.
        /// </summary>
        /// <param name="serverConfigDir"></param>
        private void CreateServerConfigIfDontExists() {
            var filesList = new List<string>() {"basic.cfg", "server.cfg", "common.Arma3Profile", "common.json"};
            foreach (var fileName in filesList) {
                var destFileName = Path.Join(_serverConfigDir, fileName);
                if (File.Exists(destFileName)) continue;
                Console.WriteLine($"{fileName} not found, copying.");
                File.Copy(Path.Join(Directory.GetCurrentDirectory(), $"example_{fileName}"), destFileName);
            }
        }

        /// <summary>
        /// If modset config.json is not present, create one with hostNamePattern.
        /// </summary>
        private void CreateModsetConfigIfDontExists() {
            if (!File.Exists(Path.Join(_modsetConfigDir, "config.json"))) {
                // Set hostName according to pattern
                var sampleServer = new Dictionary<string, string>();
                sampleServer.Add("hostName",
                    string.Format(Environment.GetEnvironmentVariable("hostNamePattern"), _modset.GetName()));
                var sampleJSON = new Dictionary<string, Dictionary<string, string>>();
                sampleJSON.Add("server", sampleServer);
                // Write to file
                using (StreamWriter file = File.CreateText(Path.Join(_modsetConfigDir, "config.json"))) {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, sampleJSON);
                }
            }
        }

        /// <summary>
        /// Prepares modset cfg files for server to load.
        /// </summary>
        private void PrepareModsetConfig() {
            // Apply modset config on top of default config
            var modsetConfig = new ConfigurationBuilder()
                .AddJsonFile(Path.Join(_serverConfigDir, "config.json"))
                .AddJsonFile(Path.Join(_modsetConfigDir, "config.json"))
                .Build();

            // Process configuration files for modset
            var configs = new List<string> {"server", "basic"};
            foreach (var config in configs) {
                Console.WriteLine($"Loading {config}.cfg for {_modset.GetName()} modset.");
                var cfgFile = FillCfg(File.ReadAllText($"{_serverConfigDir}\\{config}.cfg"),
                    modsetConfig.GetSection(config));
                File.WriteAllText(Path.Join(_modsetConfigDir, $"{config}.cfg"), cfgFile);
                Console.WriteLine($"{config}.cfg successfully exported to {_modsetConfigDir}");
            }
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
                    cfgFile = ReplaceValue(cfgFile, key, stringValue);
                } else {
                    cfgFile = ReplaceValue(cfgFile, key, config[key]);
                }
            }

            return cfgFile;
        }

        /// <summary>
        /// Performs replacement of a value for corresponding key in given config file (as string)
        /// </summary>
        public static string ReplaceValue(string config, string key, string value) {
            // Build regex expression with new line before key to prevent mid-line replacements
            var expression = $"\n{key}";
            if (key == "template") {
                expression = $"\t\t{key}";
            }

            // Check if $key contains '[]' eg. 'admins[]' as then it's value should be an array
            if (Regex.IsMatch(key, @"[[\]]")) {
                // Make some magic to replace [] in expression to \[\]
                expression = Regex.Replace(expression, @"\[\]", @"\[\]");
            }

            expression = $"{expression} = .*;";
            // If expression is not found, return unchanged config
            if (!Regex.IsMatch(config, expression)) {
                return config;
            }

            // Check character after "key = "
            // eg. for key = 'lobbyTimeout' and config entry:
            // lobbyTimeout = 1234
            // returns 1
            // Used to determine if there are quotation marks needed
            var quote = "";
            var match = Regex.Match(config, expression);
            if (match.Value.Substring(key.Length + 4, 1) == "\"") {
                quote = "\"";
            }

            var replacement = Regex.IsMatch(key, @"[[\]]")
                ? $"\n{key} = {{{value}}};"
                : $"\n{key} = {quote}{value}{quote};";
            config = Regex.Replace(config, expression, replacement);
            Console.WriteLine($"\"{match.ToString().Substring(1)}\" => \"{replacement.Substring(1)}\"");

            return config;
        }
    }
}