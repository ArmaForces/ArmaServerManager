using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;

namespace ArmaForces.ArmaServerManager.Features.Mods
{
    public interface IWebModsetMapper
    {
        IModset MapWebModsetToCacheModset(WebModset webModset);
    }
}
