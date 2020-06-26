using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

namespace ArmaServerManager
{
    public class Modset {
        private string _modsetName = "default";

        public string GetName() => _modsetName;
    }

    public class Server {
        private Settings _settings;
        private Modset _modset;
        private ServerConfig _armaConfig;
        private Process _serverProcess;
        public Server() {
            Console.WriteLine("Initializing Server");
            _settings = new Settings();
            _modset = new Modset();
            _armaConfig = new ServerConfig(_settings, _modset);
        }

        public bool IsServerRunning() {
            return _serverProcess != null ? true : false;
        }

        public bool Start() {
            Console.WriteLine("Starting Arma 3 Server");
            try {
                _serverProcess = Process.Start(_settings.GetServerExePath());
            } catch (NullReferenceException e) {
                Console.WriteLine(e);
                Console.WriteLine("Arma 3 Server could not be started. Path missing.");
                return false;
            }
            return true;
        }

        public void WaitUntilStarted() {
            Console.WriteLine($"Waiting for {_serverProcess} to finish startup.");
            _serverProcess.WaitForInputIdle();
        }

        public void Shutdown() {
            Console.WriteLine($"Shutting down the {_serverProcess}.");
            _serverProcess.Kill();
            _serverProcess = null;
        }
    }

    public class Settings {
        private static IConfigurationRoot _config;
        private readonly string _executable = "arma3server_x64.exe";
        private readonly string _serverPath;

        public Settings() {
            Console.WriteLine("Loading Manager Settings.");
            // Load config
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json")
                .AddEnvironmentVariables()
                .Build();
            // Load serverPath
            try {
                _serverPath = _config["serverPath"];
            } catch (NullReferenceException) {
                _serverPath = Registry.LocalMachine
                    .OpenSubKey("SOFTWARE\\WOW6432Node\\bohemia interactive\\arma 3")
                    ?.GetValue("main")
                    .ToString();
            }
        }

        public object GetSettingsValue(string key) {
            try {
                return _config[key];
            } catch (NullReferenceException) {
                return null;
            }
        }

        public string GetServerPath() {
            return _serverPath;
        }

        public string GetServerExePath()
        {
            if (_serverPath != null) return $"{_serverPath}\\{_executable}";
            return null;
        }
    }

    public class ServerConfig {
        private readonly Settings _settings;
        private readonly Modset _modset;
        private string _serverConfigDir;

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
            PrepareModsetConfig();
            
            Console.WriteLine("ServerConfig loaded.");
        }

        /// <summary>
        /// Prepares common serverConfig directory and files
        /// </summary>
        private string PrepareServerConfig() {
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
        private void PrepareModsetConfig() {
            // Get modset config directory based on serverConfig
            var modsetConfigDir = PrepareServerConfig() + $"modsetConfigs\\{_modset.GetName()}";

        }
    }

    internal class Program
    {
        static void Main(string[] args) {
            var server = new Server();
            server.Start();
            server.WaitUntilStarted();
            server.Shutdown();
        }
    }
}
