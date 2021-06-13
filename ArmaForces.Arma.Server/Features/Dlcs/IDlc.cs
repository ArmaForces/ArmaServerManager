using System;

namespace ArmaForces.Arma.Server.Features.Dlcs
{
    public interface IDlc
    {
        string Id { get; set; }
        
        string Name { get; set; }
        
        DateTime CreatedAt { get; set; }
        
        DateTime LastUpdatedAt { get; set; }
        
        int AppId { get; set; }
    }
}
