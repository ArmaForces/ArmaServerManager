using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

namespace ArmaServerManager {

    public class Settings
    {
        private static IConfigurationRoot _config;
        private readonly string _executable = "arma3server_x64.exe";
        private readonly string _serverPath;

        public Settings()
        {
            Console.WriteLine("Loading Manager Settings.");
            // Load config
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json")
                .AddEnvironmentVariables()
                .Build();
            // Load serverPath
            try
            {
                _serverPath = _config["serverPath"];
            }
            catch (NullReferenceException)
            {
                _serverPath = Registry.LocalMachine
                    .OpenSubKey("SOFTWARE\\WOW6432Node\\bohemia interactive\\arma 3")
                    ?.GetValue("main")
                    .ToString();
            }
        }

        public object GetSettingsValue(string key)
        {
            try
            {
                return _config[key];
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        public string GetServerPath()
        {
            return _serverPath;
        }

        public string GetServerExePath()
        {
            return _serverPath != null ? $"{_serverPath}\\{_executable}" : null;
        }
    }

}