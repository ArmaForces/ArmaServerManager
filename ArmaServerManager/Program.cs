using System;
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
            _settings = new Settings();
            _armaConfig = new ServerConfig(_settings);
        }

        public bool IsServerRunning() {
            return _serverProcess != null ? true : false;
        }

        public bool Start() {
            try {
                _serverProcess = Process.Start(_settings.GetServerExePath());
            } catch (NullReferenceException e) {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        public void WaitUntilStarted() {
            _serverProcess.WaitForInputIdle();
        }

        public void Shutdown() {
            _serverProcess.Kill();
            _serverProcess = null;
        }
    }

    public class Settings {
        private static IConfigurationRoot _config;
        private readonly string _executable = "arma3server.exe";
        private readonly string _serverPath;

        public Settings() {
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
            _settings = settings;
            // Load config directory and create if it not exists
            string _serverConfigDirName = _settings.GetSettingsValue("serverConfigDirName").ToString();
            _serverConfigDir = $"{_settings.GetServerPath()}\\{_serverConfigDirName}";
            if (!Directory.Exists(_serverConfigDir)) {
                Console.WriteLine($"Config directory {_serverConfigDirName} does not exists, creating.");
                Directory.CreateDirectory(_serverConfigDir);
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args) {
            var server = new Server();
            Console.WriteLine("Starting Arma 3 Server");
            server.Start();
            server.WaitUntilStarted();
            server.Shutdown();
        }
    }
}
