using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ArmaServerManager;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ArmaServerManagerTests {
    public class ServerConfigReplacerTests {
        [Fact]
        public void ReplaceValue_StringEqual() {
            var modsetConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("example_common.json")
                .Build();
            var cfgFile = File.ReadAllText($"{Directory.GetCurrentDirectory()}\\example_server.cfg");
            var key = "hostName"; // It has it's value as string with quotes
            var value = modsetConfig.GetSection("server")[key];

            var newCfgFile = ServerConfigReplacer.ReplaceValue(cfgFile, key, value);

            var match = Regex.IsMatch(newCfgFile, $"{key} = \"{value}\";");
            Assert.True(match);
        }

        [Fact]
        public void ReplaceValue_NoQuotesEqual() {
            var modsetConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("example_common.json")
                .Build();
            var cfgFile = File.ReadAllText($"{Directory.GetCurrentDirectory()}\\example_server.cfg");
            var key = "verifySignatures"; // It has it's value as number with no quotes
            var value = modsetConfig.GetSection("server")[key];

            var newCfgFile = ServerConfigReplacer.ReplaceValue(cfgFile, key, value);

            var match = Regex.IsMatch(newCfgFile, $"{key} = {value};");
            Assert.True(match);
        }

        [Fact]
        public void ReplaceValue_ArrayEqual() {
            var modsetConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("example_common.json")
                .Build();
            var cfgFile = File.ReadAllText($"{Directory.GetCurrentDirectory()}\\example_server.cfg");
            var key = "admins[]"; // It has array value with {} as array brackets
            var value = modsetConfig.GetSection("server").GetSection(key).GetChildren().ToList();
            var stringValue = String.Join(", ", value.Select(p => p.Value.ToString()));

            var newCfgFile = ServerConfigReplacer.ReplaceValue(cfgFile, key, stringValue);

            key = @"admins\[\]"; // Change for regex
            var match = Regex.IsMatch(newCfgFile, $"{key} = {{{stringValue}}};");
            Assert.True(match);
        }
    }
}