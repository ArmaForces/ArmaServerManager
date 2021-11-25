using System;

namespace ArmaForces.ArmaServerManager.Features.Modsets.DTOs
{
    public class WebDlc
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastUpdatedAt { get; set; }

        public int AppId { get; set; }
        
        public string? Directory { get; set; }
    }
}
