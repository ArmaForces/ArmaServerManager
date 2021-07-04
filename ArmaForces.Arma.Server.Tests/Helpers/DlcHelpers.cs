using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Dlcs;
using AutoFixture;

namespace ArmaForces.Arma.Server.Tests.Helpers
{
    public static class DlcHelpers
    {
        public static Dlc CreateTestDlc(Fixture fixture)
        {
            return fixture.Create<Dlc>();
        }

        public static List<Dlc> CreateDlcsList(Fixture fixture, int dlcsCount = 3)
        {
            var dlcsList = new List<Dlc>();

            for (var i = 0; i < dlcsCount; i++)
            {
                dlcsList.Add(fixture.Create<Dlc>());
            }
            
            return dlcsList;
        }
    }
}
