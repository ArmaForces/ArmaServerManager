using System;
using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using AutoFixture;

namespace ArmaForces.Arma.Server.Tests.Helpers
{
    public static class ModsetHelpers
    {
        public static Modset CreateEmptyModset(Fixture fixture) => CreateTestModset(fixture, 0);

        public static Modset CreateTestModset(Fixture fixture, int eachModTypeNumber = 5)
        {
            var modset = new Modset
            {
                Mods = new HashSet<IMod>(),
                LastUpdatedAt = fixture.Create<DateTime>(),
                Name = fixture.Create<string>(),
                WebId = fixture.Create<string>()
            };

            for (var i = 0; i < eachModTypeNumber; i++)
            {
                modset.Mods.Add(ModHelpers.CreateTestMod(fixture, ModType.ServerSide));
                modset.Mods.Add(ModHelpers.CreateTestMod(fixture, ModType.ClientSide));
                modset.Mods.Add(ModHelpers.CreateTestMod(fixture, ModType.Optional));
                modset.Mods.Add(ModHelpers.CreateTestMod(fixture, ModType.Required));
            }

            return modset;
        }

        public static Modset CreateTestModsetWithModsOfOneType(
            Fixture fixture,
            ModType modType,
            int modsNumber = 5)
        {
            var modset = new Modset
            {
                Mods = new HashSet<IMod>()
            };

            for (var i = 0; i < modsNumber; i++)
            {
                modset.Mods.Add(ModHelpers.CreateTestMod(fixture, modType));
            }

            return modset;
        }
    }
}
