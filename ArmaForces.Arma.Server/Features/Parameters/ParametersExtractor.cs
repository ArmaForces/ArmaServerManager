using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Extensions;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Parameters
{
    public class ParametersExtractor
    {
        public async Task<Result<ServerParameters>> ExtractParameters(Process process)
        {
            var commandLine = process.GetCommandLine();
            var serverParameters = new ServerParameters(
                GetProcessPath(commandLine),
                GetIsClient(commandLine),
                await GetServerPort(commandLine),
                null,
                null,
                null,
                null,
                await GetModsetName(commandLine));

            return Result.Success(serverParameters);
        }

        private static bool GetIsClient(string commandLine)
        {
            return IsFlagPresent("client", commandLine);
        }

        private static string GetProcessPath(string commandLine)
        {
            var end = commandLine.IndexOf('"', 1);
            return commandLine.Substring(0, end);
        }

        private static async Task<int> GetServerPort(string commandLine)
        {
            var portString = await ReadParameterStringValue("port", commandLine);
            return int.Parse(portString);
        }

        private static async Task<string> GetModsetName(string commandLine)
            => await ReadParameterStringValue("modsetName", commandLine);

        private static bool IsFlagPresent(string flag, string commandLine)
        {
            return commandLine.Contains($"-{flag} ");
        }

        private static async Task<string> ReadParameterStringValue(string parameter, string commandLine)
        {
            var ddd = new[] {'='};
            
            var portStringIndex = commandLine.IndexOf(parameter, StringComparison.InvariantCulture);
            if (portStringIndex == -1) return string.Empty;
            
            var builder = new StringBuilder();
            using var reader = new StringReader(commandLine.Substring(portStringIndex + parameter.Length));
            int charsRead;
            var valueStarted = false;
            do
            {
                var chars = new char[1];
                charsRead = await reader.ReadAsync(chars);
                var character = chars.First();

                if (charsRead == -1)
                    return builder.ToString();

                if (char.IsWhiteSpace(character))
                    if (valueStarted)
                        return builder.ToString();
                    else
                        continue;

                if (!valueStarted && !char.IsWhiteSpace(character) && ddd.Any(x => x == character))
                {
                    valueStarted = true;
                }
                else
                {
                    builder.Append(chars, 0, charsRead);
                }
            } while (charsRead > 0);

            return builder.ToString();
        }
    }
}
