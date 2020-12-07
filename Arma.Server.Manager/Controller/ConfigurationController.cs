using System.Threading.Tasks;
using Arma.Server.Manager.Features.Configuration;
using Arma.Server.Manager.Infrastructure.Authorization;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Arma.Server.Manager.Controller
{
    [Route("api/configuration")]
    [ApiController]
    [ApiKey]
    public class ConfigurationController : ControllerBase
    {
        private readonly string _serverConfigurationName = string.Empty;

        private readonly IServerConfigurationLogic _serverConfigurationLogic;

        public ConfigurationController(IServerConfigurationLogic serverConfigurationLogic)
        {
            _serverConfigurationLogic = serverConfigurationLogic;
        }

        [HttpGet("modset/{modsetName}")]
        public IActionResult GetModsetConfiguration(string modsetName)
        {
            var result = _serverConfigurationLogic.DownloadConfigurationFile(modsetName);

            return result.Match(
                onSuccess: Ok,
                onFailure: error => (IActionResult) BadRequest(error));
        }

        [HttpPut("modset/{modsetName}")]
        public async Task<IActionResult> PutModsetConfiguration(string modsetName, [FromForm] IFormFile file)
        {
            var result = await _serverConfigurationLogic.UploadConfigurationFile(modsetName, file);

            return result.Match(
                onSuccess: Ok,
                onFailure: error => (IActionResult) BadRequest(error));
        }

        [HttpGet("server")]
        public IActionResult GetServerConfiguration()
        {
            var result = _serverConfigurationLogic.DownloadConfigurationFile(_serverConfigurationName);

            return result.Match(
                onSuccess: Ok,
                onFailure: error => (IActionResult) BadRequest(error));
        }

        [HttpPut("server")]
        public async Task<IActionResult> PutServerConfiguration([FromForm] IFormFile file)
        {
            var result = await _serverConfigurationLogic.UploadConfigurationFile(_serverConfigurationName, file);

            return result.Match(
                onSuccess: Ok,
                onFailure: error => (IActionResult) BadRequest(error));
        }
    }
}
