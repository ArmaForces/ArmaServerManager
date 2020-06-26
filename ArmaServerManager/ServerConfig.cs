using System;
using System.Collections.Generic;
using System.IO;

namespace ArmaServerManager {
    public class ServerConfig
    {
        private readonly Settings _settings;
        private readonly Modset _modset;
        private string _serverConfigDir;

        /// <summary>
        /// Class prepares server configuration for given modset
        /// </summary>
        /// <param name="settings">Server Settings Object</param>
        /// <param name="modset">Modset object</param>
        public ServerConfig(Settings settings, Modset modset)
        {
            Console.WriteLine("Loading ServerConfig.");
            _settings = settings;
            _modset = modset;
            // Load config directory and create if it not exists
            PrepareModsetConfig();

            Console.WriteLine("ServerConfig loaded.");
        }

        /// <summary>
        /// Prepares common serverConfig directory and files
        /// </summary>
        private string PrepareServerConfig()
        {
            var serverPath = _settings.GetServerPath();
            var serverConfigDirName = _settings.GetSettingsValue("serverConfigDirName").ToString();
            _serverConfigDir = $"{serverPath}\\{serverConfigDirName}";
            if (Directory.Exists(_serverConfigDir)) return _serverConfigDir;
            Console.WriteLine($"Config directory {serverConfigDirName} does not exists, creating.");
            Directory.CreateDirectory(_serverConfigDir);
            // Check if common server configs are in place
            var filesList = new List<string>() { "basic.cfg", "server.cfg", "common.Arma3Profile", "common.json" };
            foreach (var fileName in filesList)
            {
                var filePath = $"{_serverConfigDir}\\{fileName}";
                if (File.Exists(filePath)) continue;
                Console.WriteLine($"{fileName} not found, copying.");
                File.Copy($"{Directory.GetCurrentDirectory()}\\example_{fileName}", filePath);
            }
            return _serverConfigDir;
        }

        /// <summary>
        /// Prepares modset specific config directory and files.
        /// </summary>
        private void PrepareModsetConfig()
        {
            // Get modset config directory based on serverConfig
            var modsetConfigDir = PrepareServerConfig() + $"modsetConfigs\\{_modset.GetName()}";

        }
    }

}