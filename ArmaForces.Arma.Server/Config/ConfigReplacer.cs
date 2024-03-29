using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Config {
    public class ConfigReplacer {
        private readonly ILogger<ConfigReplacer> _logger;

        public ConfigReplacer(ILogger<ConfigReplacer> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Performs replacement of a value for corresponding key in given config file (as string)
        /// </summary>
        public string ReplaceValue(string config, string key, string value) {
            // Build regex expression with new line before key to prevent mid-line replacements
            var whiteChars = key == "template"
                ? "\t\t"
                : "\n";

            var expression = $"{whiteChars}{key}";

            // Check if $key contains '[]' eg. 'admins[]' as then it's value should be an array
            if (Regex.IsMatch(key, @"[[\]]")) {
                // Make some magic to replace [] in expression to \[\]
                expression = Regex.Replace(expression, @"\[\]", @"\[\]");
            }

            expression = $"{expression} = .*;";
            // If expression is not found, return unchanged config
            if (!Regex.IsMatch(config, expression)) {
                return config;
            }

            // Check character after "key = "
            // eg. for key = 'lobbyTimeout' and config entry:
            // lobbyTimeout = 1234
            // returns 1
            // Used to determine if there are quotation marks needed
            var quote = "";
            var match = Regex.Match(config, expression);
            if (match.Value.Substring(key.Length + 4, 1) == "\"") {
                quote = "\"";
            }

            var replacement = Regex.IsMatch(key, @"[[\]]")
                ? $"{whiteChars}{key} = {{{value}}};"
                : $"{whiteChars}{key} = {quote}{value}{quote};";
            config = Regex.Replace(config, expression, replacement);
            
            _logger.LogTrace(
                "Replaced \"{oldValue}\" => \"{newValue}\"",
                match.ToString().Substring(1),
                replacement.Substring(1));

            return config;
        }
    }
}