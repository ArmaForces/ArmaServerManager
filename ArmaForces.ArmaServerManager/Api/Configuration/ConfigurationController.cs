using System;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Configuration;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Api.Configuration
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

        [HttpGet("server/cbaSettings")]
        public IActionResult GetServerCbaSettings()
        {
            throw new NotImplementedException("Downloading server CBA Settings is not supported yet.");
        }

        [HttpGet("server/cbaSettings")]
        public IActionResult PutServerCbaSettings([FromForm] IFormFile file)
        {
            throw new NotImplementedException("Uploading server CBA Settings is not supported yet.");
        }
    }
}
