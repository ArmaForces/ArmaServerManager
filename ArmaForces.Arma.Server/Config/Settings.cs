using System;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Constants;
using ArmaForces.Arma.Server.Exceptions;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;

namespace ArmaForces.Arma.Server.Config
{
    public class Settings : ISettings
    {
        private static string SettingsJsonPath { get; } = Path.Join(Directory.GetCurrentDirectory(), "settings.json");

        public string ManagerDirectory { get; } = Directory.GetCurrentDirectory();

        public string? ApiMissionsBaseUrl { get; set; }
        public string? ApiModsetsBaseUrl { get; set; }
        public string ModsetConfigDirectoryName { get; set; } = "modsetConfig";
        // TODO: Extract to IModsConfig or something
        public string? ModsDirectory { get; set; }
        public string ModsManagerCacheFileName { get; set; } = ".ManagerModsCache";
        public string? ServerConfigDirectory { get; set; }
        public string? ServerDirectory { get; set; }
        public string? ServerExecutable { get; set; }
        public string ServerExecutableName { get; set; } = "arma3server_x64.exe";
        public string? SteamUser { get; set; }
        public string? SteamPassword { get; set; }
        public string? WebhookUrl { get; set; }

        private readonly IFileSystem _fileSystem;
        private readonly IRegistryReader _registryReader;
        private IConfigurationRoot _config;

        // TODO: Create factory for this too
        public Settings():this(null, null, null){}

        public Settings(
            IConfigurationRoot? config = null,
            IFileSystem? fileSystem = null,
            IRegistryReader? registryReader = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
            _config = config ?? LoadConfigFile();
            _registryReader = registryReader ?? new RegistryReader();

            LoadSettings();
        }

        public static Settings LoadSettings(IServiceProvider serviceProvider)
        {
            var json = File.ReadAllTextAsync(SettingsJsonPath).Result;
            return JsonSerializer.Deserialize<Settings>(json);
        }

        public Result LoadSettings()
        {
            Console.WriteLine("Loading Manager Settings.");
            return GetServerPath()
                .Tap(GetServerExecutable)
                .Tap(ObtainModsDirectory)
                .Tap(ObtainServerConfigDirectory)
                .Tap(ObtainApiMissionsBaseUrl)
                .Tap(ObtainApiModsetsBaseUrl)
                .Tap(ObtainSteamUserName)
                .Tap(ObtainSteamPassword)
                .Tap(ObtainWebhookUrl);
        }

        public Result ReloadSettings()
        {
            _config = LoadConfigFile();
            return LoadSettings();
        }

        public async Task<Result> ReloadSettings(ISettings settings)
        {
            ApiMissionsBaseUrl = settings.ApiMissionsBaseUrl;
            ApiModsetsBaseUrl = settings.ApiModsetsBaseUrl;
            ModsetConfigDirectoryName = settings.ModsetConfigDirectoryName;
            ModsDirectory = settings.ModsDirectory;
            ModsManagerCacheFileName = settings.ModsManagerCacheFileName;
            ServerConfigDirectory = settings.ServerConfigDirectory;
            ServerDirectory = settings.ServerDirectory;
            ServerExecutable = settings.ServerExecutable;
            ServerExecutableName = settings.ServerExecutableName;
            SteamUser = settings.SteamUser;
            SteamPassword = settings.SteamPassword;
            WebhookUrl = settings.WebhookUrl;

            var json = JsonSerializer.Serialize(this, JsonOptions.Default);
            await _fileSystem.File.WriteAllTextAsync(SettingsJsonPath, json);

            return Result.Success();
        }

        private IConfigurationRoot LoadConfigFile()
            => new ConfigurationBuilder()
                .SetBasePath(_fileSystem.Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json")
                .AddEnvironmentVariables()
                .Build();

        private void ObtainServerConfigDirectory()
            => ServerConfigDirectory ??= _config["serverConfigDirectory"] ?? Path.Join(ServerDirectory, "serverConfig");

        private void ObtainModsDirectory()
            => ModsDirectory ??= _config["modsDirectory"] ?? Path.Join(ServerDirectory, "mods");

        private void ObtainApiMissionsBaseUrl() => ApiMissionsBaseUrl ??= _config["apiMissionsBaseUrl"];

        private void ObtainApiModsetsBaseUrl() => ApiModsetsBaseUrl ??= _config["apiModsetsBaseUrl"];

        private void ObtainSteamUserName() => SteamUser ??= _config["steamUserName"];

        private void ObtainSteamPassword() => SteamPassword ??= _config["steamPassword"];
        
        private void ObtainWebhookUrl() => WebhookUrl ??= _config["webhookUrl"];

        private Result GetServerPath()
        {
            var serverPath = GetServerPathFromConfig() ?? GetServerPathFromRegistry();
            ServerDirectory ??= serverPath ?? throw new ServerNotFoundException("Could not find server directory.");
            return Result.Success();
        }

        private string? GetServerPathFromConfig()
        {
            var serverPath = _config["serverDirectory"];
            return _fileSystem.Directory.Exists(serverPath)
                ? serverPath
                : null;
        }

        private string? GetServerPathFromRegistry()
        {
            try
            {
                var serverPath = _registryReader
                    .GetValueFromLocalMachine(@"SOFTWARE\WOW6432Node\bohemia interactive\arma 3", "main")?
                    .ToString();

                return _fileSystem.Directory.Exists(serverPath)
                    ? serverPath
                    : null;
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        private void GetServerExecutable()
        {
            var serverExecutableName = _config["serverExecutableName"];
            var serverExecutablePath = Path.Join(ServerDirectory, serverExecutableName);
            ServerExecutable ??= _fileSystem.File.Exists(serverExecutablePath)
                ? serverExecutablePath
                : Path.Join(ServerDirectory, "arma3server_x64.exe");
        }
    }
}
