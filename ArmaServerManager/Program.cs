using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

namespace ArmaServerManager
{
    public class Settings {
        private static readonly IConfigurationRoot Config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("settings.json")
            .AddEnvironmentVariables()
            .Build();

        public string executable = "arma3server.exe";
        public string RegistryPath = Registry.LocalMachine
            .OpenSubKey("SOFTWARE\\WOW6432Node\\bohemia interactive\\arma 3")
            ?.GetValue("main")
            .ToString();
        public string ServerPath = Config["serverPath"];

        public string GetServerPath() {
            return RegistryPath + "\\" + executable;
        }
    }

    internal class Program
    {
        static void Main(string[] args) {
            var serverSettings = new Settings();
            Console.WriteLine("Hello World!");
            var server =
                Process.Start(serverSettings.GetServerPath());
            server.WaitForInputIdle();
            Console.WriteLine(server.ProcessName);
            string serverMainWindowTitle = server.MainWindowTitle;
            Console.WriteLine(serverMainWindowTitle);
            server.Kill();
        }
    }
}
