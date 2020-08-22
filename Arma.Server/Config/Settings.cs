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

        public Result LoadSettings() {
            Console.WriteLine("Loading Manager Settings.");
            return LoadConfigFile()
                .Bind(GetServerPath)
                .Tap((() => ServerExecutable = Path.Join(ServerDirectory, ServerExecutableName)))
                .Tap(ObtainModsDirectory);
        }

        private Result LoadConfigFile()
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json")
                .AddEnvironmentVariables()
                .Build();
            return Result.Success();
        }

        private Result GetServerPath() {
            string serverPath = null;
            var serverPathLoaded = GetServerPathFromConfig()
                .Tap(x => serverPath = x)
                .OnFailure(x => GetServerPathFromRegistry())
                .Tap(x => serverPath = x)
                .OnFailure(e => throw new ServerNotFoundException(e));
            if (serverPathLoaded.IsFaulted)
            {
                throw serverPathLoaded.Exception.GetBaseException();
            }

            ServerDirectory = serverPath;
            return Result.Success();
        }

        private void ObtainModsDirectory()
            => ModsDirectory = _config["modsDirectory"] ?? Path.Join(ServerDirectory, "mods");

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