using System.Collections.Generic;
using System.Linq;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Tests.Features.Modsets.TestingHelpers
{
    [ApiController]
    [Route("api/mod-lists")]
    public class ModsetsTestController : ControllerBase
    {
        private readonly ModsetsStorage _modsetsStorage;

        public const string ModsetWithIdNotExistsMessage = "Modset with given ID does not exist.";
        public const string ModsetWithNameDoesNotExistMessage = "Modset with given name does not exist.";

        public ModsetsTestController(ModsetsStorage modsetsStorage)
        {
            _modsetsStorage = modsetsStorage;
        }

        [HttpGet]
        public ActionResult<List<WebModset>> GetModsets()
        {
            return Ok(_modsetsStorage.Modsets);
        }

        [HttpGet("by-name/{name}")]
        public ActionResult<WebModset> GetModsetByName(string name)
        {
            var modset = _modsetsStorage.Modsets.SingleOrDefault(x => x.Name == name);

            return modset != null
                ? Ok(modset)
                : (ActionResult<WebModset>) NotFound(ModsetWithNameDoesNotExistMessage);
        }

        [HttpGet("{id}")]
        public ActionResult<WebModset> GetModsetById(string id)
        {
            var modset = _modsetsStorage.Modsets.SingleOrDefault(x => x.Id == id);

            return modset != null
                ? Ok(modset)
                : (ActionResult<WebModset>) NotFound(ModsetWithIdNotExistsMessage);
        }
    }
}
