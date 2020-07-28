using System;
using System.IO;
using ArmaServerManager;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace ArmaServerManagerTests {
    public class ModsetConfigTests: IDisposable {
        private const string ModsetName = "default";
        private const string ServerConfigDirName = "TestDir";
        private readonly string ServerConfigDirPath;
        private readonly string ModsetConfigDirPath;

        public ModsetConfigTests() {
            ServerConfigDirPath = Path.Join(Directory.GetCurrentDirectory(), ServerConfigDirName);
            ModsetConfigDirPath = Path.Join(ServerConfigDirPath, "modsetConfigs", "default");
        }

        public void Dispose() {
            Directory.Delete(ServerConfigDirPath, true);
        }

        [Fact]
        public void ModsetConfig_LoadConfig_Success() {
            var settingsMock = new Mock<ISettings>();
            settingsMock.Setup(settings => settings.GetServerPath()).Returns(Directory.GetCurrentDirectory());
            settingsMock.Setup(settings => settings.GetSettingsValue("serverConfigDirName")).Returns(ServerConfigDirName);

            var modsetConfig = new ModsetConfig(settingsMock.Object, ModsetName);
            var configLoaded = modsetConfig.LoadConfig();
            
            Assert.True(configLoaded.IsSuccess);
            Assert.True(Directory.Exists(ModsetConfigDirPath));
            Assert.True(File.Exists(Path.Join(ModsetConfigDirPath, "server.cfg")));
            Assert.True(File.Exists(Path.Join(ModsetConfigDirPath, "basic.cfg")));
            Assert.True(File.Exists(Path.Join(ModsetConfigDirPath, "config.json")));
        }
    }
}