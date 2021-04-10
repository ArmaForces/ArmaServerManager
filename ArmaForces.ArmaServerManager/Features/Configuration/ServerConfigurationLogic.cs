using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArmaForces.ArmaServerManager.Features.Configuration
{
    public class ServerConfigurationLogic : IServerConfigurationLogic
    {
        private const string ServerConfigJsonFileName = "common.json";
        private const string ModsetConfigJsonFileName = "config.json";

        private readonly IFileSystem _fileSystem;

        private string ModsetConfigsPath { get; }
        private string ServerConfigPath { get; }

        public ServerConfigurationLogic(ISettings settings) : this(settings, new FileSystem()) { }

        internal ServerConfigurationLogic(ISettings settings, IFileSystem fileSystem)
        {
            ServerConfigPath = settings.ServerConfigDirectory!;
            ModsetConfigsPath = Path.Join(settings.ServerConfigDirectory, settings.ModsetConfigDirectoryName);
            _fileSystem = fileSystem;
        }
        
        public async Task<Result<string>> UploadConfigurationFile(string modsetName, IFormFile formFile)
        {
            var (configDirectory, configFileName) = GetConfigDirectoryAndFileName(modsetName);
            
            return await ValidateJsonFile(formFile)
                .Bind(() => CheckDirectoryExists(configDirectory))
                .Bind(directoryPath => GetJsonConfigFilePath(directoryPath, configFileName))
                .Bind(filePath => CopyToFile(filePath, formFile))
                .Bind(() => Result.Success(modsetName));
        }

        public Result<Stream> DownloadConfigurationFile(string modsetName)
        {
            var (configDirectory, configFileName) = GetConfigDirectoryAndFileName(modsetName);

            return GetJsonConfigFilePath(configDirectory, configFileName)
                .Bind(CheckFileExists)
                .Bind(GetFileStream);
        }

        private static Result ValidateJsonFile(IFormFile formFile)
        {
            if (formFile.Length == 0)
                return Result.Failure<string>("File is empty.");

            if (formFile.ContentType != "application/json")
                return Result.Failure<string>("Configuration file must be in json format.");

            using var stream = formFile.OpenReadStream();
            using var streamReader = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(streamReader);
            var jsonSerializer = new JsonSerializer();

            try
            {
                while (jsonTextReader.Read())
                {
                    var d = jsonSerializer.Deserialize<JObject>(jsonTextReader);
                }
            }
            catch
            {
                return Result.Failure("Json file is incorrect.");
            }

            return Result.Success();
        }

        private async Task<Result> CopyToFile(string filePath, IFormFile formFile)
        {
            await using var stream = _fileSystem.FileStream.Create(filePath, FileMode.Create);
            await formFile.CopyToAsync(stream);

            return Result.Success();
        }

        private ValueTuple<string, string> GetConfigDirectoryAndFileName(string modsetName)
            => string.IsNullOrWhiteSpace(modsetName)
                ? (Path.Join(ServerConfigPath), ServerConfigJsonFileName)
                : (Path.Join(ModsetConfigsPath, modsetName), ModsetConfigJsonFileName);

        private Result<Stream> GetFileStream(string filePath)
            => Result.Success(_fileSystem.FileStream.Create(filePath, FileMode.Open));

        private Result<string> GetJsonConfigFilePath(string modsetConfigDirectory, string configFileName)
            => Result.Success(_fileSystem.Path.Join(modsetConfigDirectory, configFileName));

        private Result<string> CheckFileExists(string filePath)
            => _fileSystem.File.Exists(filePath)
                ? filePath
                : Result.Failure<string>("File does not exist.");

        private Result<string> CheckDirectoryExists(string directoryPath) 
            => _fileSystem.Directory.Exists(directoryPath)
                ? directoryPath 
                : Result.Failure<string>("Modset directory does not exist.");
    }
}
