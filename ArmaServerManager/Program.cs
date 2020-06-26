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

        public string GetServerExePath() {
            return _serverPath != null ? $"{_serverPath}\\{_executable}" : null;
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
