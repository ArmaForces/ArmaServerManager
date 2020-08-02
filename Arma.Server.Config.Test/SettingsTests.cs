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
        public void Settings_ServerExePath_NotNull() {
            // Act
            Settings serverSettings = new Settings();

            // Assert
            Assert.NotNull(serverSettings.GetServerExePath());
        }

        [Fact]
        public void Settings_NoServerPresent_ThrowsException() {
            // Arrange
            Action action = () => new Settings();

            // Act & Assert
            action.Should().Throw<ServerNotFoundException>().WithMessage(@"Server path could not be loaded*");
        }
    }
}