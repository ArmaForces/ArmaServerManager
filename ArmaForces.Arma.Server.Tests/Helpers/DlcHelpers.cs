using System.Collections.Generic;
using System.Linq;
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
            => fixture.CreateMany<Dlc>(dlcsCount)
                .ToList();
    }
}
