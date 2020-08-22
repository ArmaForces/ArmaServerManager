using System;
using System.IO;
using System.IO.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

namespace Arma.Server.Config {
    public class Settings : ISettings {
        public string ModlistConfigDirectoryName { get; protected set; } = "modlistConfig";
        public string ModsDirectory { get; protected set; }
        public string ServerConfigDirectory { get; protected set; }
        public string ServerDirectory { get; protected set; }
        public string ServerExecutable { get; protected set; }
        public string ServerExecutableName { get; protected set; } = "arma3server_x64.exe";

        private readonly IConfigurationRoot _config;
        private IFileSystem _fileSystem;
        private IRegistryReader _registryReader;

        public Settings(IConfigurationRoot config = null,
            IFileSystem fileSystem = null,
            IRegistryReader registryReader = null) {
            _fileSystem = fileSystem ?? new FileSystem();
            _config = config ?? LoadConfigFile();
            _registryReader = registryReader ?? new RegistryReader();
        }

        public Result LoadSettings() {
            Console.WriteLine("Loading Manager Settings.");
            return GetServerPath()
                .Tap(GetServerExecutable)
                .Tap(ObtainModsDirectory)
                .Tap(ObtainServerConfigDirectory);
        }

        private IConfigurationRoot LoadConfigFile()
            => new ConfigurationBuilder()
                .SetBasePath(_fileSystem.Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json")
                .AddEnvironmentVariables()
                .Build();

        private void ObtainServerConfigDirectory()
            => ServerConfigDirectory = _config["serverConfigDirectory"] ?? Path.Join(ServerDirectory, "serverConfig");

        private void ObtainModsDirectory()
            => ModsDirectory = _config["modsDirectory"] ?? Path.Join(ServerDirectory, "mods");

        private Result GetServerPath() {
            string serverPath = GetServerPathFromConfig() ?? GetServerPathFromRegistry();
            ServerDirectory = serverPath ?? throw new ServerNotFoundException("Could not find server directory.");
            return Result.Success();
        }

        private string GetServerPathFromConfig() {
            var serverPath = _config["serverDirectory"];
            return _fileSystem.Directory.Exists(serverPath)
                ? serverPath
                : null;
        }

        private string GetServerPathFromRegistry() {
            try {
                var serverPath = _registryReader
                    .GetValueFromLocalMachine(@"SOFTWARE\WOW6432Node\bohemia interactive\arma 3", "main")
                    .ToString();
                return _fileSystem.Directory.Exists(serverPath)
                    ? serverPath
                    : null;
            } catch (NullReferenceException) {
                return null;
            }
        }

        private void GetServerExecutable() {
            string serverExecutableName = _config["serverExecutableName"];
            string serverExecutablePath = Path.Join(ServerDirectory, serverExecutableName);
            ServerExecutable = _fileSystem.File.Exists(serverExecutablePath)
                ? serverExecutablePath
                : Path.Join(ServerDirectory, "arma3server_x64.exe");
        }
    }
}