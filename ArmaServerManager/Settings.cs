using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

namespace ArmaServerManager {
    public interface ISettings
    {
        object GetSettingsValue(string key);
        string GetServerPath();
        string GetServerExePath();
    }

    public class Settings : ISettings
    {
        private static IConfigurationRoot _config;
        private readonly string _executable = "arma3server_x64.exe";
        private readonly string _serverPath;

        public Settings(IConfigurationRoot config) {
            Console.WriteLine("Loading Manager Settings.");
            _config = config;
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
}