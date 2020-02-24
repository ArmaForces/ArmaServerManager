using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

namespace ArmaServerManager
{
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

        public string GetServerExePath()
        {
            if (_serverPath != null) return $"{_serverPath}\\{_executable}";
            return null;
        }
    }

    internal class Program
    {
        static void Main(string[] args) {
            var serverSettings = new Settings();
            Console.WriteLine("Starting Arma 3 Server");
            var server =
                Process.Start(serverSettings.GetServerExePath());
            server.WaitForInputIdle();
            Console.WriteLine(server.ProcessName);
            string serverMainWindowTitle = server.MainWindowTitle;
            Console.WriteLine(serverMainWindowTitle);
            server.Kill();
        }
    }
}
