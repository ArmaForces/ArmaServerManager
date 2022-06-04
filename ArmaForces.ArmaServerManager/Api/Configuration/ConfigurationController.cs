using System;
using System.Net.Mime;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Configuration;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Api.Configuration
{
    /// <summary>
    /// Allows server configuration manipulation.
    /// </summary>
    [Route("api/configuration")]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [ApiKey]
    public class ConfigurationController : ControllerBase
    {
        private readonly string _serverConfigurationName = string.Empty;

        private readonly IServerConfigurationLogic _serverConfigurationLogic;

        /// <inheritdoc />
        public ConfigurationController(IServerConfigurationLogic serverConfigurationLogic)
        {
            _serverConfigurationLogic = serverConfigurationLogic;
        }

        /// <summary>Get Modset Configuration</summary>
        /// <remarks>Retrieves server configuration for given <paramref name="modsetName"/>.</remarks>
        /// <param name="modsetName">Name of modset to retrieve configuration for.</param>
        [HttpGet("modset/{modsetName}", Name = nameof(GetModsetConfiguration))]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetModsetConfiguration(string modsetName)
        {
            var result = _serverConfigurationLogic.DownloadConfigurationFile(modsetName);

            return result.Match(
                onSuccess: Ok,
                onFailure: error => (IActionResult) BadRequest(error));
        }

        /// <summary>Put Modset Configuration</summary>
        /// <remarks>Sets configuration file for given <paramref name="modsetName"/>.</remarks>
        /// <param name="modsetName">Name of modset to set configuration for.</param>
        /// <param name="file">New configuration file.</param>
        [HttpPut("modset/{modsetName}", Name = nameof(PutModsetConfiguration))]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PutModsetConfiguration(string modsetName, [FromForm] IFormFile file)
        {
            var result = await _serverConfigurationLogic.UploadConfigurationFile(modsetName, file);

            return result.Match(
                onSuccess: Ok,
                onFailure: error => (IActionResult) BadRequest(error));
        }

        /// <summary>Get Server Configuration</summary>
        /// <remarks>Retrieves global server configuration.</remarks>
        [HttpGet("server", Name = nameof(GetServerConfiguration))]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetServerConfiguration()
        {
            var result = _serverConfigurationLogic.DownloadConfigurationFile(_serverConfigurationName);

            return result.Match(
                onSuccess: Ok,
                onFailure: error => (IActionResult) BadRequest(error));
        }
        
        /// <summary>Set Server Configuration</summary>
        /// <remarks>Sets global server configuration.</remarks>
        /// <param name="file">New global configuration file.</param>
        [HttpPut("server", Name = nameof(PutServerConfiguration))]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PutServerConfiguration([FromForm] IFormFile file)
        {
            var result = await _serverConfigurationLogic.UploadConfigurationFile(_serverConfigurationName, file);

            return result.Match(
                onSuccess: Ok,
                onFailure: error => (IActionResult) BadRequest(error));
        }
        
        /// <summary>Get Server CBA Settings</summary>
        /// <remarks>Proposed. Not implemented.</remarks>
        [HttpGet("server/cbaSettings", Name = nameof(GetServerCbaSettings))]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult GetServerCbaSettings()
        {
            throw new NotImplementedException("Downloading server CBA Settings is not supported yet.");
        }

        /// <summary>Put Server CBA Settings</summary>
        /// <remarks>Proposed. Not implemented.</remarks>
        [HttpPut("server/cbaSettings", Name = nameof(PutServerCbaSettings))]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult PutServerCbaSettings([FromForm] IFormFile file)
        {
            throw new NotImplementedException("Uploading server CBA Settings is not supported yet.");
        }
    }
}
