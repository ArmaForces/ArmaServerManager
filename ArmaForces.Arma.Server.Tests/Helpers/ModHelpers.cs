using System;
using System.Collections.Generic;
using ArmaForces.Arma.Server.Mod;
using AutoFixture;

namespace ArmaForces.Arma.Server.Tests.Helpers
{
    public static class ModHelpers
    {
        public static Mod.Mod CreateTestMod(Fixture fixture)
        {
            return fixture.Create<Mod.Mod>();
        }

        public static Mod.Mod CreateTestMod(Fixture fixture, ModType modType)
        {
            return new Mod.Mod
            {
                CreatedAt = fixture.Create<DateTime>(),
                Directory = fixture.Create<string>(),
                LastUpdatedAt = fixture.Create<DateTime>(),
                Name = fixture.Create<string>(),
                Source = fixture.Create<ModSource>(),
                Type = modType,
                WebId = fixture.Create<string>(),
                WorkshopId = fixture.Create<int>()
            };
        }

        public static List<Mod.Mod> CreateModsList(Fixture fixture, int modsNumber)
        {
            var modsList = new List<Mod.Mod>();

            for (var i = 0; i < modsNumber; i++)
            {
                modsList.Add(fixture.Create<Mod.Mod>());
            }

            return modsList;
        }
    }
}
