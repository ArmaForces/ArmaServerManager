using System;

namespace ArmaForces.Arma.Server.Features.Dlcs
{
    public class Dlc : IDlc
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime LastUpdatedAt { get; set; }
        
        public int AppId { get; set; }
    }
}
