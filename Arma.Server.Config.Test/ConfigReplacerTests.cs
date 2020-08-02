using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Moq;
using Xunit;

namespace Arma.Server.Config.Test {
    public class ConfigReplacerTests {
        private readonly string _cfgFile = String.Join("\r\n",
                                        "", // Necessary that first line is not any param
                                        "hostName = \"\"; // Servername visible in the game browser. Default: hostName = \"\"",
                                        "admins[] = {\"\"}; //e.g. {\"1234\",\"5678\"} whitelisted client can use #login w/o password (since Arma 3 1.69+). Default: admins[] = {\"\"}",
                                        "verifySignatures = 0; // Enables or disables the signature verification for addons. Default: verifySignatures = 3, disabled = 0");

        private readonly string JsonFile = String.Join("\r\n",
                                        "{",
                                            "\"basic\": {",
                                                "\"MinBandwidth\": 131072,",
                                                "\"MaxBandwidth\": 52428800,",
                                                "\"MaxMsgSend\": 128,",
                                                "\"MaxSizeGuaranteed\": 512,",
                                                "\"MaxSizeNonguaranteed\": 256,",
                                                "\"MinErrorToSend\": 0.001,",
                                                "\"MinErrorToSendNear\": 0.01,",
                                                "\"MaxCustomFileSize\": 1310720",
                                            "},",
                                            "\"server\": {",
                                                "\"hostName\": \"My fun server\",",
                                                "\"password\": \"nopassword\",",
                                                "\"passwordAdmin\": \"noadminpassword\",",
                                                "\"serverCommandPassword\": \"nocommandpassword\",",
                                                "\"admins[]\": [\"\\\"123\\\"\",\"\\\"456\\\"\"]",
                                            "}",
                                        "}");

        private readonly IConfigurationRoot _modsetConfig;

        public ConfigReplacerTests() {
            // Create configuration from JsonFile string
            var jsonStreamConfigurationSource = new JsonStreamConfigurationSource();
            var configurationBuilder = new ConfigurationBuilder();

            jsonStreamConfigurationSource.Stream = JsonFile.As<Stream>();
            jsonStreamConfigurationSource.Build(configurationBuilder);

            _modsetConfig = configurationBuilder.Build();
        }

        [Fact]
        public void ReplaceValue_StringFromConfiguration_Match() {
            // Arrange
            var key = "hostName"; // It has array value with {} as array brackets
            var value = _modsetConfig.GetSection("server")[key];
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
            var key = "verifySignatures"; // It has array value with {} as array brackets
            var value = _modsetConfig.GetSection("server")[key];
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
            var value = _modsetConfig.GetSection("server").GetSection(key).GetChildren().ToList();
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