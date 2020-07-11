using ArmaServerManager;
using Xunit;

namespace ArmaServerManagerTests {
    public class ServerConfigTests {
        [Fact]
        public void ServerConfig_Init_Success() {
            var serverConfig = new ServerConfig(new Settings(), new Modset().GetName());
            Assert.True(true);
        }
    }
}