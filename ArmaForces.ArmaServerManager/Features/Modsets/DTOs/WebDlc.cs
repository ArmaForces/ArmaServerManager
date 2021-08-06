using System;

namespace ArmaForces.ArmaServerManager.Features.Modsets.DTOs
{
    public class WebDlc
    {
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime LastUpdatedAt { get; set; }

        public int AppId { get; set; }
        
        public string? Directory { set; get; }
    }
}
