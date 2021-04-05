using System;
using System.Linq;

namespace ArmaForces.ArmaServerManager.Features.Missions.DTOs {
    public class WebMission
    {
        public string Title { get; set; } = string.Empty;

        public DateTime Date { get; set; }
        
        public DateTime CloseDate { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Modlist {
            get => _modlist;
            set => _modlist = value.Split('/').Last();
        }

        public bool Archive { get; set; }

        private string _modlist = string.Empty;
    }
}