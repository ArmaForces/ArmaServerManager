using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Constants;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.Arma.Server.Features.Keys;
using ArmaForces.Arma.Server.Features.Keys.Models;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Tests.Helpers;
using ArmaForces.Arma.Server.Tests.Helpers.Extensions;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Features.Keys
{
    public class KeysPreparerIntegrationTests
    {
        private readonly Fixture _fixture = new Fixture();
        
        [Fact, Trait("Category", "Integration")]
        public void PrepareKeysForModset_MultipleModTypesAllWithBikeys_CorrectBikeysCopied()
        {
            var settings = new TestSettings();
            var fileSystem = CreateFileSystemMock(settings);
            var keysDirectory = MockUnixSupport.Path($"{settings.ServerDirectory}\\{KeysConstants.KeysDirectoryName}");
            var modsDirectory = settings.ModsDirectory!;

            var config = new ServerConfig(settings, fileSystem);
            var modset = ModsetHelpers.CreateTestModset(_fixture, modsDirectory);
            foreach (var mod in modset.Mods)
            {
                CreateDirectoryForMod(fileSystem, mod);
            }

            var serviceProvider = CreateServiceProvider(settings, config, fileSystem);

            var keysPreparer = serviceProvider.GetService<IKeysPreparer>()!;

            var result = keysPreparer.PrepareKeysForModset(modset);

            using (new AssertionScope())
            {
                result.ShouldBeSuccess();
                if (result.IsFailure)
                {
                    result.Error.Should().BeNullOrEmpty();
                }
                
                var expectedBikeyNames = GetBikeysForMods(fileSystem, modset.ClientLoadableMods)
                    .Append(new BikeyFile("a3.bikey"))
                    .Select(x => x.FileName)
                    .ToList();

                var copiedBikeys = GetCopiedBikeys(fileSystem, keysDirectory)
                    .Select(x => x.FileName)
                    .ToList();

                copiedBikeys.Should().BeEquivalentTo(expectedBikeyNames);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public void PrepareKeysForModset_SingleModWithoutKeyWithExternalKey_ExternalBikeyCopied()
        {
            var settings = new TestSettings();
            var fileSystem = CreateFileSystemMock(settings);
            var keysDirectory = MockUnixSupport.Path($"{settings.ServerDirectory}\\{KeysConstants.KeysDirectoryName}");
            var modsDirectory = settings.ModsDirectory!;
            var externalKeysDirectory = MockUnixSupport.Path($"{settings.ServerConfigDirectory}\\{KeysConstants.KeysDirectoryName}");

            var config = new ServerConfig(settings, fileSystem);
            var mod = ModHelpers.CreateTestMod(
                _fixture,
                ModType.Required,
                modsDirectory);
            CreateDirectoryForMod(fileSystem, mod, createBikey: false);
            
            var modset = ModsetHelpers.CreateModsetWithMods(_fixture, mod.AsList());
            CreateExternalKeyForMod(fileSystem, externalKeysDirectory, mod);

            var serviceProvider = CreateServiceProvider(settings, config, fileSystem);

            var keysPreparer = serviceProvider.GetService<IKeysPreparer>()!;

            var result = keysPreparer.PrepareKeysForModset(modset);

            using (new AssertionScope())
            {
                result.ShouldBeSuccess();
                if (result.IsFailure)
                {
                    result.Error.Should().BeNullOrEmpty();
                }
                
                var expectedBikeyNames = new List<BikeyFile>
                {
                    new BikeyFile("a3.bikey"),
                    new BikeyFile($"{mod.Name}{KeysConstants.KeyExtension}")
                }
                    .Select(x => x.FileName)
                    .ToList();

                var copiedBikeys = GetCopiedBikeys(fileSystem, keysDirectory)
                    .Select(x => x.FileName)
                    .ToList();

                copiedBikeys.Should().BeEquivalentTo(expectedBikeyNames);
            }
        }

        private static void CreateExternalKeyForMod(
            IFileSystem fileSystem,
            string externalKeysDirectory,
            IMod mod)
        {
            var externalKeysDirectoryForMod = fileSystem.Path.Join(externalKeysDirectory, mod.Directory!.Split("\\").Last());
            fileSystem.Directory.CreateDirectory(externalKeysDirectoryForMod);

            var keyFileName = $"{mod.Name}{KeysConstants.KeyExtension}";
            fileSystem.File.Create(fileSystem.Path.Join(externalKeysDirectoryForMod, keyFileName));
        }

        private IFileSystem CreateFileSystemMock(ISettings settings)
        {
            var fileSystemMock = new MockFileSystem();

            fileSystemMock.Directory.CreateDirectory(settings.ManagerDirectory);
            fileSystemMock.File.Create(fileSystemMock.Path.Join(settings.ManagerDirectory, KeysConstants.ArmaKey));
            
            fileSystemMock.Directory.CreateDirectory(settings.ServerDirectory);
            fileSystemMock.Directory.CreateDirectory(settings.ModsDirectory);
            fileSystemMock.Directory.CreateDirectory(
                fileSystemMock.Path.Join(settings.ServerDirectory, KeysConstants.KeysDirectoryName));

            return fileSystemMock;
        }

        private List<BikeyFile> GetCopiedBikeys(
            IFileSystem fileSystem,
            string keysDirectory)
        {
            return fileSystem.Directory.GetFiles(keysDirectory)
                .Select(path => new BikeyFile(path))
                .ToList();
        }

        private List<BikeyFile> GetBikeysForMods(IFileSystem fileSystem, ISet<IMod> mods)
        {
            return mods
                .Select(mod => fileSystem.Directory.GetFiles(
                    mod.Directory,
                    $"*{KeysConstants.KeyExtension}",
                    SearchOption.AllDirectories))
                .SelectMany(x => x)
                .Select(x => new BikeyFile(x))
                .ToList();
        }

        private void CreateDirectoryForMod(
            IFileSystem fileSystem,
            IMod mod,
            bool createBikey = true)
        {
            var modDirectoryPath = mod.Directory;
            fileSystem.Directory.CreateDirectory(modDirectoryPath);
            
            var modKeysPath = fileSystem.Path.Join(modDirectoryPath, KeysConstants.KeysDirectoryName);
            fileSystem.Directory.CreateDirectory(modKeysPath);

            if (createBikey)
            {
                var bikeyFilePath = fileSystem.Path.Join(
                    modKeysPath,
                    $"{_fixture.Create<string>()}{KeysConstants.KeyExtension}");
                fileSystem.File.Create(bikeyFilePath);
            }
        }

        private static IServiceProvider CreateServiceProvider(
            ISettings settings,
            IConfig config,
            IFileSystem fileSystem)
        {
            return new ServiceCollection()
                .AddLogging()
                .AddArmaServer()
                .Replace(new ServiceDescriptor(typeof(ISettings), settings))
                .Replace(new ServiceDescriptor(typeof(IConfig), config))
                .AddSingleton(fileSystem)
                .BuildServiceProvider();
        }
    }
}
