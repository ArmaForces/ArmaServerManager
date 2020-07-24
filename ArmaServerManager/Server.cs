﻿using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ArmaServerManager {
    public class Server {
        private Settings _settings;
        private Modset _modset;
        private ServerConfig _armaConfig;
        private Process _serverProcess;

        public Server() {
            Console.WriteLine("Initializing Server");
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json")
                .AddEnvironmentVariables()
                .Build();
            _settings = new Settings(config);
            _modset = new Modset();
            _armaConfig = new ServerConfig(_settings, _modset.GetName());
            _armaConfig.LoadConfig();
        }

        public bool IsServerRunning() {
            return _serverProcess != null;
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
}