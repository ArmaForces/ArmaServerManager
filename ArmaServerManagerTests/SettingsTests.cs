using ArmaServerManager;
using Xunit;

namespace ArmaServerManagerTests
{
    public class SettingsTests
    {
        [Fact]
        public void Settings_ServerExePath_NotNull()
        {
            Settings serverSettings = new Settings();
            Assert.NotNull(serverSettings.GetServerExePath());
        }
    }
}
