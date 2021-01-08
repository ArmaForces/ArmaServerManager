using System;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace ArmaForces.ArmaServerManager.Extensions
{
    internal static class ProcessExtensions
    {
        /// <summary>
        /// Returns command line of given <paramref name="process"/>.
        /// </summary>
        /// <param name="process">Process to get it's command line.</param>
        /// <returns>Command line string or null.</returns>
        public static string GetCommandLine(this Process process)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                using var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id);
                using var objects = searcher.Get();

                return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
            }

            // TODO: Add for Linux
            return null;
        }
    }
}
