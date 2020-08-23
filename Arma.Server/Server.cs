using System;
using System.Diagnostics;
using Arma.Server.Config;

namespace Arma.Server {
    public class Server {
        private ISettings _settings;
        private Process _serverProcess;
        private IModsetConfig _modsetConfig;

        public Server(ISettings settings, IModsetConfig modsetConfig) {
            _settings = settings;
            _modsetConfig = modsetConfig;
            Console.WriteLine("Initializing Server");
        }

        public bool IsServerRunning() {
            return _serverProcess != null;
        }

        public bool Start() {
            Console.WriteLine("Starting Arma 3 Server");
            try {
                _serverProcess = Process.Start(_settings.ServerExecutable, GetServerStartupParams());
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
            if (_serverProcess == null)
                return;
            Console.WriteLine($"Shutting down the {_serverProcess}.");
            _serverProcess.Kill();
            _serverProcess = null;
        }

        private string GetServerStartupParams() {
            return String.Join(' ',
                "-port=2302",
                $"\"-config={_modsetConfig.ServerCfg}\"",
                $"\"-cfg={_modsetConfig.BasicCfg}\"",
                $"-profiles=\"{_modsetConfig.ServerProfileDirectory}\"",
                "-name=server",
                "-filePatching",
                "-netlog",
                "-limitFPS=100",
                "-loadMissionToMemory");
        }
    }
}