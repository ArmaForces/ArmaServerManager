using System.IO;
using ArmaServerManager;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ArmaServerManagerTests {
    public class SettingsTests {
        [Fact]
        public void Settings_ServerExePath_NotNull() {
            Settings serverSettings = new Settings(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json")
                .AddEnvironmentVariables()
                .Build());
            Assert.NotNull(serverSettings.GetServerExePath());
        }
    }
}