using System;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using Moq;
using Xunit;

namespace Arma.Server.Config.Test {
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