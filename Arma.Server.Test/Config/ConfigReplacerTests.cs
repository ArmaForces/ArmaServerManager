﻿using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Arma.Server.Config;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Arma.Server.Test.Config {
    public class ConfigReplacerTests {
        private readonly string _cfgFile = File.ReadAllText(Path.Join(Directory.GetCurrentDirectory(), "test_server.cfg"));

        private readonly IConfigurationRoot _modlistConfig;

        public ConfigReplacerTests() {
            // Create configuration from JsonFile string
            //var jsonStreamConfigurationSource = new JsonStreamConfigurationSource();
            _modlistConfig = new ConfigurationBuilder()
                .AddJsonFile(Path.Join(Directory.GetCurrentDirectory(),"test_common.json"))
                .Build();

            var jsonString = File.ReadAllText(Path.Join(Directory.GetCurrentDirectory(), "test_common.json"));
            var JSON = JsonSerializer.Deserialize<Object>(jsonString);
        }

        [Fact]
        public void ReplaceValue_StringFromConfiguration_Match() {
            // Arrange
            var key = "hostName"; // It has it's value as string with quotes
            var value = _modlistConfig.GetSection("server")[key];
            var expectedMatch = $"{key} = \"{value}\";";

            // Act
            var newCfgFile = ConfigReplacer.ReplaceValue(_cfgFile, key, value);

            // Assert
            newCfgFile.Should().Contain(expectedMatch);
        }

        [Fact]
        public void ReplaceValue_String_Match() {
            // Arrange
            var key = "hostName"; // It has it's value as string with quotes
            var value = "My fun server";
            var expectedMatch = $"{key} = \"{value}\";";

            // Act
            var newCfgFile = ConfigReplacer.ReplaceValue(_cfgFile, key, value);

            // Assert
            newCfgFile.Should().Contain(expectedMatch);
        }

        [Fact]
        public void ReplaceValue_NoQuotesFromConfiguration_Match() {
            // Arrange
            var key = "verifySignatures"; // It has it's value as number with no quotes
            var value = _modlistConfig.GetSection("server")[key];
            var expectedMatch = $"{key} = {value};";

            // Act
            var newCfgFile = ConfigReplacer.ReplaceValue(_cfgFile, key, value);

            // Assert
            newCfgFile.Should().Contain(expectedMatch);
        }

        [Fact]
        public void ReplaceValue_NoQuotes_Match() {
            // Arrange
            var key = "verifySignatures"; // It has it's value as number with no quotes
            var value = 3.ToString();
            var expectedMatch = $"{key} = {value};";

            // Act
            var newCfgFile = ConfigReplacer.ReplaceValue(_cfgFile, key, value);

            // Assert
            newCfgFile.Should().Contain(expectedMatch);
        }

        [Fact]
        public void ReplaceValue_ArrayFromConfiguration_Match() {
            // Arrange
            var key = "admins[]"; // It has array value with {} as array brackets
            var value = _modlistConfig.GetSection("server").GetSection(key).GetChildren().ToList();
            var stringValue = String.Join(", ", value.Select(p => p.Value.ToString()));
            var expectedMatch = $"{key} = {{{stringValue}}};";

            // Act
            var newCfgFile = ConfigReplacer.ReplaceValue(_cfgFile, key, stringValue);

            // Assert
            newCfgFile.Should().Contain(expectedMatch);
        }

        [Fact]
        public void ReplaceValue_ArrayString_Match() {
            // Arrange
            var key = "admins[]"; // It has array value with {} as array brackets
            var stringValue = "\"123\", \"456\"";
            var expectedMatch = $"{key} = {{{stringValue}}};";

            // Act
            var newCfgFile = ConfigReplacer.ReplaceValue(_cfgFile, key, stringValue);

            // Assert
            newCfgFile.Should().Contain(expectedMatch);
        }
    }
}