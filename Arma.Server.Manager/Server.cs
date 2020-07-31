using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ArmaServerManager {
    public class Server {
        private Settings _settings;
        private Modset _modset;
        private Process _serverProcess;
        private ModsetConfig _modsetConfig;

        public Server() {
            Console.WriteLine("Initializing Server");
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json")
                .AddEnvironmentVariables()
                .Build();
            _settings = new Settings(config);
            _modset = new Modset();
            _modsetConfig = new ModsetConfig(_settings, _modset.GetName());
            _modsetConfig.LoadConfig();
        }

        public bool IsServerRunning() {
            return _serverProcess != null;
        }

        public bool Start() {
            Console.WriteLine("Starting Arma 3 Server");
            try {
                _serverProcess = Process.Start(_settings.GetServerExePath(), GetServerStartupParams());
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

        private string GetServerStartupParams() {
            return String.Join(' ',
                "-port=2302",
                $"\"-config={_modsetConfig.GetServerCfgPath()}\"",
                $"\"-cfg={_modsetConfig.GetBasicCfgPath()}\"",
                $"-profiles=\"{_modsetConfig.GetServerProfileDir()}\"",
                "-name=server",
                "-filePatching",
                "-netlog",
                "-limitFPS=100",
                "-loadMissionToMemory");
        }
    }
}