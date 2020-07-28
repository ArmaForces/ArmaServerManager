using System.IO;
using ArmaServerManager;
using Moq;
using Xunit;

namespace ArmaServerManagerTests {
    public class ModsetConfigTests {
        private const string ModsetName = "default";
        private const string ServerConfigDirName = "TestDir";

        [Fact]
        public void ModsetConfig_LoadConfig_Success() {
            var modsetConfigDirPath = Path.Join(Directory.GetCurrentDirectory(), ServerConfigDirName, "modsetConfigs", "default");
            var settingsMock = new Mock<ISettings>();
            settingsMock.Setup(settings => settings.GetServerPath()).Returns(Directory.GetCurrentDirectory());
            settingsMock.Setup(settings => settings.GetSettingsValue("serverConfigDirName")).Returns(ServerConfigDirName);

            var modsetConfig = new ModsetConfig(settingsMock.Object, ModsetName);
            var configLoaded = modsetConfig.LoadConfig();
            
            Assert.True(configLoaded.IsSuccess);
            Assert.True(Directory.Exists(modsetConfigDirPath));
            Assert.True(File.Exists(Path.Join(modsetConfigDirPath, "server.cfg")));
            Assert.True(File.Exists(Path.Join(modsetConfigDirPath, "basic.cfg")));
            Assert.True(File.Exists(Path.Join(modsetConfigDirPath, "config.json")));

            // Clear
            Directory.Delete(modsetConfigDirPath, true);
        }
    }
}