using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Arma.Server.Config
{
    public class ConfigFileCreator
    {
        /// <summary>
        /// In given cfgFile string replaces values for keys present in given config. Returns filled string.
        /// </summary>
        public static string FillCfg(string cfgFile, IConfigurationSection config) {
            foreach (var section in config.GetChildren()) {
                var key = section.Key;
                var value = config.GetSection(key).GetChildren().ToList();
                // Replace default value in cfg with value from loaded configs
                if (value.Count != 0) {
                    // If value is array, it needs changing to string
                    var stringValue = String.Join(", ", value.Select(p => p.Value.ToString()));
                    cfgFile = ConfigReplacer.ReplaceValue(cfgFile, key, stringValue);
                } else {
                    cfgFile = ConfigReplacer.ReplaceValue(cfgFile, key, config[key]);
                }
            }

            return cfgFile;
        }
    }
}
