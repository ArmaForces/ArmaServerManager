using ArmaServerManager;
using System;
using Xunit;

namespace ArmaServerManagerTests
{
    public class UnitTest1
    {
        [Fact]
        public void Settings_ServerExePath_NotNull()
        {
            Settings serverSettings = new Settings();
            Assert.NotNull(serverSettings.GetServerExePath());
        }
    }
}
