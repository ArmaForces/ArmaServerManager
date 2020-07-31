namespace ArmaServerManager {
    internal class Program {
        static void Main(string[] args) {
            var server = new Server();
            server.Start();
            server.WaitUntilStarted();
            server.Shutdown();
        }
    }
}