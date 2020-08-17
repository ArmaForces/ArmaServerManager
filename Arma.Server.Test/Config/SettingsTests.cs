using Arma.Server.Config;
using Xunit;

namespace Arma.Server.Test.Config {
    public class SettingsTests {

        [Fact]
        public void Settings_ServerPath_FoundOrThrewException() {
            try {
                // Act
                Settings serverSettings = new Settings();
                // Assert
                Assert.NotNull(serverSettings.GetServerExePath());
            } catch (ServerNotFoundException e) {
                // Assert if exception
                Assert.Contains(@"Server path could not be loaded", e.Message);
            }
        }
    }
}