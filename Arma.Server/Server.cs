using System;
using System.Diagnostics;
using Arma.Server.Config;

namespace Arma.Server {
    public class Server {
        private ISettings _settings;
        private Process _serverProcess;
        private IModlistConfig _modlistConfig;

        public Server(ISettings settings, IModlistConfig modlistConfig) {
            _settings = settings;
            _modlistConfig = modlistConfig;
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
                $"\"-config={_modlistConfig.ServerCfg}\"",
                $"\"-cfg={_modlistConfig.BasicCfg}\"",
                $"-profiles=\"{_modlistConfig.ServerProfileDirectory}\"",
                "-name=server",
                "-filePatching",
                "-netlog",
                "-limitFPS=100",
                "-loadMissionToMemory");
        }
    }
}