using ArmaServerManager;
using System;
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

    public class ServerTests {
        [Fact]
        public void Server_IsRunning_Success() {
            Server server = new Server();
            server.Start();
            Assert.True(server.IsServerRunning());
        }
    }
}
