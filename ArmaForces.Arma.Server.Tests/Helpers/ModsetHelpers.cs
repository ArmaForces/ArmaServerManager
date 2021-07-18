using System;
using System.Collections.Generic;
using System.Linq;
using ArmaForces.Arma.Server.Features.Dlcs;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using AutoFixture;

namespace ArmaForces.Arma.Server.Tests.Helpers
{
    public static class ModsetHelpers
    {
        public static Modset CreateEmptyModset(Fixture fixture) => CreateTestModset(fixture, eachModTypeNumber: 0);

        public static Modset CreateModsetWithMods(Fixture fixture, IReadOnlyCollection<IMod> mods)
        {
            return new Modset
            {
                Mods = mods.ToHashSet(),
                LastUpdatedAt = fixture.Create<DateTime>(),
                Name = fixture.Create<string>(),
                WebId = fixture.Create<string>()
            };
        }
        
        public static Modset CreateTestModset(
            Fixture fixture,
            string? modsDirectory = null,
            int eachModTypeNumber = 5)
        {
            var modset = new Modset
            {
                Mods = new HashSet<IMod>(),
                Dlcs = new HashSet<Dlc>(),
                LastUpdatedAt = fixture.Create<DateTime>(),
                Name = fixture.Create<string>(),
                WebId = fixture.Create<string>()
            };

            for (var i = 0; i < eachModTypeNumber; i++)
            {
                modset.Mods.Add(ModHelpers.CreateTestMod(fixture, ModType.ServerSide, modsDirectory));
                modset.Mods.Add(ModHelpers.CreateTestMod(fixture, ModType.ClientSide, modsDirectory));
                modset.Mods.Add(ModHelpers.CreateTestMod(fixture, ModType.Optional, modsDirectory));
                modset.Mods.Add(ModHelpers.CreateTestMod(fixture, ModType.Required, modsDirectory));
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

        public static Modset CopyModset(Modset modset)
        {
            return new Modset
            {
                LastUpdatedAt = modset.LastUpdatedAt,
                Mods = modset.Mods.ToHashSet(),
                Dlcs = modset.Dlcs.ToHashSet(),
                Name = modset.Name,
                WebId = modset.WebId
            };
        }
    }
}
