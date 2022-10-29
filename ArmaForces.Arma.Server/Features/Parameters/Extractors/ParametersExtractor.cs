using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Constants;
using ArmaForces.Arma.Server.Extensions;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Parameters.Extractors
{
    internal class ParametersExtractor : IParametersExtractor
    {
        public async Task<Result<ProcessParameters>> ExtractParameters(Process process)
            => await ExtractParameters(process.GetCommandLine());

        public async Task<Result<ProcessParameters>> ExtractParameters(string? commandLine)
        {
            if (commandLine is null)
            {
                return Result.Failure<ProcessParameters>("Command line is null, not possible to extract parameters.");
            }

            // TODO: Read mods too
            var serverParameters = new ProcessParameters(
                GetProcessPath(commandLine),
                GetIsClient(commandLine),
                await GetServerPort(commandLine),
                await GetServerCfgPath(commandLine),
                await GetBasicCfgPath(commandLine),
                await GetProfilePath(commandLine),
                await GetName(commandLine),
                await GetModsetName(commandLine),
                await GetStartTime(commandLine),
                GetIsFilePatchingEnabled(commandLine),
                GetIsNetLogEnabled(commandLine),
                await GetFpsLimit(commandLine),
                GetLoadMissionToMemory(commandLine),
                await GetConnectIpAddress(commandLine));

            return Result.Success(serverParameters);
        }

        private static async Task<string> GetServerCfgPath(string commandLine)
        {
            return await ReadParameterStringValue("config", commandLine);
        }

        private static async Task<string> GetBasicCfgPath(string commandLine)
        {
            return await ReadParameterStringValue("cfg", commandLine);
        }

        private static async Task<string> GetProfilePath(string commandLine)
        {
            return await ReadParameterStringValue("profiles", commandLine);
        }

        private static async Task<string> GetName(string commandLine)
        {
            return await ReadParameterStringValue("name", commandLine);
        }

        private static bool GetLoadMissionToMemory(string commandLine)
        {
            return IsFlagPresent("loadMissionToMemory", commandLine);
        }

        private static async Task<int> GetFpsLimit(string commandLine)
        {
            var fps = await ReadParameterStringValue("limitFPS", commandLine);
            return int.TryParse(fps, out var result)
                ? result
                : ParametersDefaults.LimitFPS;
        }

        private static bool GetIsNetLogEnabled(string commandLine)
        {
            // ReSharper disable once StringLiteralTypo
            return IsFlagPresent("netlog", commandLine);
        }

        private static bool GetIsFilePatchingEnabled(string commandLine)
        {
            return IsFlagPresent("filePatching", commandLine);
        }

        private static bool GetIsClient(string commandLine)
        {
            return IsFlagPresent("client", commandLine);
        }

        private static string GetProcessPath(string commandLine)
        {
            var end = commandLine.IndexOf('"', 1);
            return commandLine.Substring(0, end).Trim('"');
        }

        private static async Task<int> GetServerPort(string commandLine)
        {
            var portString = await ReadParameterStringValue("port", commandLine);
            return int.Parse(portString);
        }

        private static async Task<string> GetModsetName(string commandLine)
            => await ReadParameterStringValue("modsetName", commandLine);

        private static async Task<DateTimeOffset?> GetStartTime(string commandLine)
        {
            var dateTimeString = await ReadParameterStringValue("startTime", commandLine);
            return string.IsNullOrEmpty(dateTimeString)
                ? (DateTimeOffset?) null
                : DateTimeOffset.Parse(dateTimeString);
        }

        private static async Task<string> GetConnectIpAddress(string commandLine)
        {
            return await ReadParameterStringValue("connect", commandLine);
        }

        private static bool IsFlagPresent(string flag, string commandLine)
        {
            return commandLine.Contains($"-{flag}");
        }

        private static async Task<string> ReadParameterStringValue(string parameter, string commandLine)
        {
            var ddd = new[] {'=', '"'};
            parameter = $"-{parameter}";
            
            var stringIndex = commandLine.IndexOf(parameter, StringComparison.InvariantCulture);
            if (stringIndex == -1) return string.Empty;
            
            var builder = new StringBuilder();
            using var reader = new StringReader(commandLine.Substring(stringIndex + parameter.Length));
            int charsRead;
            var valueStarted = false;
            var isQuoted = commandLine.Substring(stringIndex - 1, 1) == "\"";
            do
            {
                var chars = new char[1];
                charsRead = await reader.ReadAsync(chars);
                var character = chars.First();

                if (charsRead == -1)
                    return builder.ToString();

                if (char.IsWhiteSpace(character) && !isQuoted)
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
                    if (isQuoted && character == '"')
                    {
                        return builder.ToString();
                    }

                    if (character == '"')
                    {
                        isQuoted = true;
                    }
                    else
                    {
                        builder.Append(chars, 0, charsRead);
                    }
                }
            } while (charsRead > 0);

            return builder.ToString();
        }
    }
}
