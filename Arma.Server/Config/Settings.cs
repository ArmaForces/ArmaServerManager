using System;
using System.IO;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

namespace Arma.Server.Config {
    public class Settings : ISettings {
        public string ModlistConfigDirectoryName { get; protected set; } = "modlistConfig";
        public string ModsDirectory { get; protected set; }
        public string ServerConfigDirectoryName { get; protected set; } = "serverConfig";
        public string ServerDirectory { get; protected set; }
        public string ServerExecutable { get; protected set; }
        public string ServerExecutableName { get; protected set; } = "arma3server_64.exe";

        private static IConfigurationRoot _config;

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

            ServerDirectory = serverPath;
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
            return Directory.Exists(serverPath)
                ? Result.Success(serverPath)
                : Result.Failure<string>("Server path could not be loaded from config.");
        }

        private Result<string> GetServerPathFromRegistry() {
            var serverPath = Registry.LocalMachine
                .OpenSubKey("SOFTWARE\\WOW6432Node\\bohemia interactive\\arma 3")
                ?.GetValue("main")
                .ToString();
            return Directory.Exists(serverPath)
                ? Result.Success(serverPath)
                : Result.Failure<string>("Server path could not be loaded from registry.");
        }
    }
}