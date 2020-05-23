using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

namespace ArmaServerManager
{
    public class Server {
        private Settings _settings;
        private ServerConfig _armaConfig;
        private Process _serverProcess;
        public Server() {
            Console.WriteLine("Initializing Server");
            _settings = new Settings();
            _armaConfig = new ServerConfig(_settings);
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
        private readonly string _executable = "arma3server.exe";
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
        private string _serverConfigDir;
        public ServerConfig(Settings settings) {
            Console.WriteLine("Loading ServerConfig.");
            _settings = settings;
            // Load config directory and create if it not exists
            string serverPath = settings.GetServerPath();
            string serverConfigDirName = _settings.GetSettingsValue("serverConfigDirName").ToString();
            _serverConfigDir = $"{serverPath}\\{serverConfigDirName}";
            if (!Directory.Exists(_serverConfigDir)) {
                Console.WriteLine($"Config directory {serverConfigDirName} does not exists, creating.");
                Directory.CreateDirectory(_serverConfigDir);
            }
            // Check if common server configs are in place
            var filesList = new List<string>() {"basic.cfg", "server.cfg", "common.Arma3Profile", "common.json"};
            foreach (var fileName in filesList) {
                string filePath = $"{_serverConfigDir}\\{fileName}";
                if (!File.Exists(filePath)) {
                    Console.WriteLine($"{fileName} not found, copying.");
                    File.Copy($"{Directory.GetCurrentDirectory()}\\example_{fileName}", filePath);
                }
            }
            Console.WriteLine("ServerConfig loaded.");
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
