using System;
using System.Linq;

namespace Arma.Server.Manager.Clients.Missions.Entities {
    public class WebMission {
        public string Title { get; set; }

        public DateTime Date { get; set; }
        
        public DateTime CloseDate { get; set; }

        public string Description { get; set; }

        public string Modlist {
            get => _modlist;
            set => _modlist = value.Split('/').Last();
        }

        public bool Archive { get; set; }

        private string _modlist;
    }
}