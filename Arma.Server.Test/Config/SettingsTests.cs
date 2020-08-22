using Arma.Server.Config;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Xunit;

namespace Arma.Server.Test.Config {
    public class SettingsTests {

        [Fact]
        public void Settings_ServerPath_FoundOrThrewException() {
            try {
                // Act
                ISettings serverSettings = new Settings();
                var loaded = serverSettings.LoadSettings();
                // Assert
                loaded.Should().BeEquivalentTo(Result.Success());
                serverSettings.ServerDirectory.Should().NotBeNullOrEmpty();
            } catch (ServerNotFoundException e) {
                // Assert if exception
                Assert.Contains(@"Server path could not be loaded", e.Message);
            }
        }
    }
}