using System;
using Arma.Server.Mod;
using AutoFixture;

namespace Arma.Server.Test.Helpers
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
    }
}
