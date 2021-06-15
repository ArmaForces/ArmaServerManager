using System;
using System.Collections.Generic;
using System.IO;
using ArmaForces.Arma.Server.Features.Mods;
using AutoFixture;

namespace ArmaForces.Arma.Server.Tests.Helpers
{
    public static class ModHelpers
    {
        public static Mod CreateTestMod(Fixture fixture)
        {
            return fixture.Create<Mod>();
        }
       
        public static Mod CreateTestMod(
            Fixture fixture,
            ModType modType,
            string? modBaseDirectory = null)
        {
            var modName = fixture.Create<string>();
            
            return new Mod
            {
                CreatedAt = fixture.Create<DateTime>(),
                Directory = Path.Join(modBaseDirectory, modName),
                LastUpdatedAt = fixture.Create<DateTime>(),
                Name = modName,
                Source = fixture.Create<ModSource>(),
                Type = modType,
                WebId = fixture.Create<string>(),
                WorkshopId = fixture.Create<int>()
            };
        }

        public static List<Mod> CreateModsList(Fixture fixture, int modsNumber)
        {
            var modsList = new List<Mod>();

            for (var i = 0; i < modsNumber; i++)
            {
                modsList.Add(fixture.Create<Mod>());
            }

            return modsList;
        }
    }
}
