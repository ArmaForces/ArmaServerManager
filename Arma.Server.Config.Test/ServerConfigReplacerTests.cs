using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using Moq;
using Xunit;

namespace Arma.Server.Config.Test {
    public class ServerConfigReplacerTests {
        private readonly string _cfgFile = String.Join("\r\n",
            "", // Necessary that first line is not any param
            "hostName = \"\"; // Servername visible in the game browser.",
            "admins[] = {\"\"}; //e.g. {\"1234\",\"5678\"} whitelisted client can use #login w/o password (since Arma 3 1.69+).",
            "verifySignatures = 0; // Enables or disables the signature verification for addons. Default = 2, disabled = 0"
        );
        
        private const string JsonFile = "{\r\n\t\"basic\": {" +
                                            "\r\n\t\t\"MinBandwidth\": 131072," +
                                            "\r\n\t\t\"MaxBandwidth\": 52428800," +
                                            "\r\n        \r\n\t\t\"MaxMsgSend\": 128," +
                                            "\r\n\t\t\"MaxSizeGuaranteed\": 512," +
                                            "\r\n\t\t\"MaxSizeNonguaranteed\": 256," +
                                            "\r\n\r\n\t\t\"MinErrorToSend\": 0.001," +
                                            "\r\n\t\t\"MinErrorToSendNear\": 0.01," +
                                            "\r\n\t\t\"MaxCustomFileSize\": 1310720\r\n\t}," +
                                        "\r\n\r\n\t\"server\": {" +
                                            "\r\n\t\t\"hostName\": \"My fun server\"," +
                                            "\r\n\t\t\"password\": \"nopassword\"," +
                                            "\r\n\t\t\"passwordAdmin\": \"noadminpassword\"," +
                                            "\r\n\t\t\"serverCommandPassword\": \"nocommandpassword\"," +
                                            "\r\n\t\t\"admins[]\": [\"\\\"123\\\"\",\"\\\"456\\\"\"]\r\n" +
                                            "\t}\r\n" +
                                        "}";

        private readonly IConfigurationRoot _modsetConfig;

        public ServerConfigReplacerTests() {
            var jsonStreamConfigurationSource = new JsonStreamConfigurationSource();
            var configurationBuilder = new ConfigurationBuilder();

            jsonStreamConfigurationSource.Stream = JsonFile.As<Stream>();
            jsonStreamConfigurationSource.Build(configurationBuilder);

            _modsetConfig = configurationBuilder.Build();
        }

        [Fact]
        public void ReplaceValue_StringFromConfiuration_Match()
        {
            var key = "hostName"; // It has array value with {} as array brackets
            var value = _modsetConfig.GetSection("server")[key];
            var expectedMatch = $"{key} = \"{value}\";";

            var newCfgFile = ServerConfigReplacer.ReplaceValue(_cfgFile, key, value);

            newCfgFile.Should().Contain(expectedMatch);
        }

        [Fact]
        public void ReplaceValue_String_Match() {
            var key = "hostName"; // It has it's value as string with quotes
            var value = "My fun server";
            var expectedMatch = $"{key} = \"{value}\";";

            var newCfgFile = ServerConfigReplacer.ReplaceValue(_cfgFile, key, value);

            newCfgFile.Should().Contain(expectedMatch);
        }

        [Fact]
        public void ReplaceValue_NoQuotesFromConfiuration_Match()
        {
            var key = "verifySignatures"; // It has array value with {} as array brackets
            var value = _modsetConfig.GetSection("server")[key];
            var expectedMatch = $"{key} = {value};";

            var newCfgFile = ServerConfigReplacer.ReplaceValue(_cfgFile, key, value);

            newCfgFile.Should().Contain(expectedMatch);
        }

        [Fact]
        public void ReplaceValue_NoQuotes_Match() {
            var key = "verifySignatures"; // It has it's value as number with no quotes
            var value = 3.ToString();
            var expectedMatch = $"{key} = {value};";

            var newCfgFile = ServerConfigReplacer.ReplaceValue(_cfgFile, key, value);

            newCfgFile.Should().Contain(expectedMatch);
        }

        [Fact]
        public void ReplaceValue_ArrayFromConfiuration_Match()
        {
            var key = "admins[]"; // It has array value with {} as array brackets
            var value = _modsetConfig.GetSection("server").GetSection(key).GetChildren().ToList();
            var stringValue = String.Join(", ", value.Select(p => p.Value.ToString()));
            var expectedMatch = $"{key} = {{{stringValue}}};";

            var newCfgFile = ServerConfigReplacer.ReplaceValue(_cfgFile, key, stringValue);

            newCfgFile.Should().Contain(expectedMatch);
        }

        [Fact]
        public void ReplaceValue_ArrayString_Match() {
            var key = "admins[]"; // It has array value with {} as array brackets
            var stringValue = "\"123\", \"456\"";
            var expectedMatch = $"{key} = {{{stringValue}}};";

            var newCfgFile = ServerConfigReplacer.ReplaceValue(_cfgFile, key, stringValue);

            newCfgFile.Should().Contain(expectedMatch);
        }
    }
}