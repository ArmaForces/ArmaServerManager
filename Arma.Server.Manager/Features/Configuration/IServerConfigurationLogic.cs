using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;

namespace Arma.Server.Manager.Features.Configuration
{
    public interface IServerConfigurationLogic
    {
        Task<Result<string>> UploadConfigurationFile(string modsetName, IFormFile formFile);

        Result<Stream> DownloadConfigurationFile(string modsetName);
    }
}
