using System;
using System.Collections.Generic;
using ArmaForces.Arma.Server.Mod;
using AutoFixture;

namespace ArmaForces.Arma.Server.Tests.Helpers
{
    public static class ModsetHelpers
    {
        public static Modset.Modset CreateEmptyModset(Fixture fixture) => CreateTestModset(fixture, 0);

        public static Modset.Modset CreateTestModset(Fixture fixture, int eachModTypeNumber = 5)
        {
            var modset = new Modset.Modset
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

        public static Modset.Modset CreateTestModsetWithModsOfOneType(
            Fixture fixture,
            ModType modType,
            int modsNumber = 5)
        {
            var modset = new Modset.Modset
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
