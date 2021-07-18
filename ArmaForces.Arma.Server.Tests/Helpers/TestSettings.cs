using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Tests.Helpers
{
    public class TestSettings : ISettings
    {
        public string? ApiMissionsBaseUrl { get; set; }
        public string? ApiModsetsBaseUrl { get; set; }
        public string ManagerDirectory => MockUnixSupport.Path($"{ServerDirectory}\\Manager");
        public string ModsetConfigDirectoryName { get; set; } = "ModsetConfig";
        public string? ModsDirectory => MockUnixSupport.Path($"{ServerDirectory}\\Mods");
        public string ModsManagerCacheFileName { get; set; } = string.Empty;
        public string? ServerConfigDirectory => MockUnixSupport.Path($"{ServerDirectory}\\ServerConfig");
        public string? ServerDirectory { get; set; } = MockUnixSupport.Path("C:\\Arma");
        public string? ServerExecutable => MockUnixSupport.Path($"{ServerDirectory}\\{ServerExecutableName}");
        public string ServerExecutableName { get; set; } = "arma3.exe";
        public string? SteamUser { get; set; }
        public string? SteamPassword { get; set; }
        
        public Result LoadSettings() => throw new System.NotImplementedException();

        public Result ReloadSettings() => throw new System.NotImplementedException();

        public async Task<Result> ReloadSettings(ISettings settings) => throw new System.NotImplementedException();
    }
}
