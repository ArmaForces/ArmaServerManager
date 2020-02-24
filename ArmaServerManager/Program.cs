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

        private readonly string _executable = "arma3server.exe";
        
        private readonly string _serverPath = SetServerPath();

        private static string SetServerPath()
        {
            try {
                return Config["serverPath"];
            } catch (NullReferenceException) {
                return Registry.LocalMachine
                .OpenSubKey("SOFTWARE\\WOW6432Node\\bohemia interactive\\arma 3")
                ?.GetValue("main")
                .ToString();
            };
        }

        public string GetServerExe() => _serverPath + "\\" + _executable;
    }

    internal class Program
    {
        static void Main(string[] args) {
            var serverSettings = new Settings();
            Console.WriteLine("Starting Arma 3 Server");
            var server =
                Process.Start(serverSettings.GetServerExe());
            server.WaitForInputIdle();
            Console.WriteLine(server.ProcessName);
            string serverMainWindowTitle = server.MainWindowTitle;
            Console.WriteLine(serverMainWindowTitle);
            server.Kill();
        }
    }
}
