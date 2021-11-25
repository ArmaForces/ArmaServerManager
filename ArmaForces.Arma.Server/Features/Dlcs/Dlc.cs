using ArmaForces.Arma.Server.Features.Dlcs.Constants;
using ArmaForces.Arma.Server.Features.Mods;

namespace ArmaForces.Arma.Server.Features.Dlcs
{
    public class Dlc : Mod
    {
        public string Id { get; set; } = string.Empty;
        
        public override ModSource Source => ModSource.SteamWorkshop;
        
        public override ModType Type => ModType.Required;
        
        public override long? WorkshopId => (long) AppId;
        
        public DlcAppId AppId { get; set; }
    }
}
