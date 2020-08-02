using System;
using System.IO;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

namespace Arma.Server.Config {
  public class Settings : ISettings
    {
        private static IConfigurationRoot _config;
        private readonly string _executable = "arma3server_x64.exe";
        private readonly string _serverPath;

        public Settings() {
            Console.WriteLine("Loading Manager Settings.");
            _config = LoadConfigFile();
            string serverPath = null;
            var serverPathLoaded = GetServerPathFromConfig()
                .Tap(x => serverPath = x)
                .OnFailure(x => GetServerPathFromRegistry())
                .Tap(x => serverPath = x)
                .OnFailure(e => throw new ServerNotFoundException(e));
            if (serverPathLoaded.IsFaulted) {
                throw serverPathLoaded.Exception.GetBaseException();
            }
            _serverPath = serverPath;
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

        private IConfigurationRoot LoadConfigFile() {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json")
                .AddEnvironmentVariables()
                .Build();
        }

        private Result<string> GetServerPathFromConfig() {
            var serverPath = _config["serverPath"];
            return Directory.Exists(serverPath) ? Result.Success(serverPath) : Result.Failure<string>("Server path could not be loaded from config.");
        }

        private Result<string> GetServerPathFromRegistry() {
            var serverPath = Registry.LocalMachine
                .OpenSubKey("SOFTWARE\\WOW6432Node\\bohemia interactive\\arma 3")
                ?.GetValue("main")
                .ToString();
            return Directory.Exists(serverPath) ? Result.Success(serverPath) : Result.Failure<string>("Server path could not be loaded from registry.");
        }
    }
}