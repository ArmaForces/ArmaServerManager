using ArmaServerManager;
using Xunit;

namespace ArmaServerManagerTests {
    public class ServerTests
    {
        [Fact]
        public void Server_IsRunningBeforeStart_Success() {
            Server server = new Server();
            Assert.False(server.IsServerRunning());
        }

        [Fact]
        public void Server_IsRunningAfterStart_Success() {
            Server server = new Server();
            server.Start();
            Assert.True(server.IsServerRunning());
            server.Shutdown();
        }

        [Fact]
        public void Server_Shutdown_Success() {
            Server server = new Server();
            server.Start();
            server.WaitUntilStarted();
            server.Shutdown();
            Assert.False(server.IsServerRunning());
        }
    }
}