using System;
using System.Collections.Generic;
using System.IO;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using AutoFixture;
using AutoFixture.Kernel;

namespace ArmaForces.ArmaServerManager.Tests.Features.Modsets.TestingHelpers
{
    public static class WebModsetHelper
    {
        public static WebModset CreateTestModset(
            ISpecimenBuilder fixture,
            string? modsDirectory = null,
            int eachModTypeNumber = 5)
        {
            var modset = new WebModset
            {
                Mods = new List<WebMod>(),
                Dlcs = new List<WebDlc>(),
                LastUpdatedAt = fixture.Create<DateTime>(),
                Name = fixture.Create<string>(),
                Id = fixture.Create<string>()
            };

            for (var i = 0; i < eachModTypeNumber; i++)
            {
                modset.Mods.Add(CreateTestMod(fixture, WebModType.ServerSide, modsDirectory));
                modset.Mods.Add(CreateTestMod(fixture, WebModType.ClientSide, modsDirectory));
                modset.Mods.Add(CreateTestMod(fixture, WebModType.Optional, modsDirectory));
                modset.Mods.Add(CreateTestMod(fixture, WebModType.Required, modsDirectory));
            }

            return modset;
        }

        private static WebMod CreateTestMod(
            ISpecimenBuilder fixture,
            WebModType modType,
            string? modsDirectory)
        {
            var modName = fixture.Create<string>();
            var modSource = fixture.Create<WebModSource>();

            return new WebMod
            {
                Id = fixture.Create<string>(),
                Name = modName,
                CreatedAt = fixture.Create<DateTime>(),
                LastUpdatedAt = fixture.Create<DateTime>(),
                Source = modSource,
                Type = modType,
                // TODO: Handle itemId can be null for non steam mods
                ItemId = fixture.Create<long>(),
                Directory = modsDirectory is null
                    ? null
                    : Path.Join(modsDirectory, modName),
            };
        }
    }
}
