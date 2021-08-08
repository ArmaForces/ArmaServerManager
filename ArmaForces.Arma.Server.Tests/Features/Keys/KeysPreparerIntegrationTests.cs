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
using ArmaForces.Arma.Server.Tests.Helpers;
using ArmaForces.Arma.Server.Tests.Helpers.Extensions;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Features.Keys
{
    public class KeysPreparerIntegrationTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly ISettings _settings = new TestSettings();
        private readonly IFileSystem _fileSystem;
        private readonly IServiceProvider _serviceProvider;

        private readonly string _keysDirectory;
        private readonly string _externalKeysDirectory;
        private readonly string _modsDirectory;

        public KeysPreparerIntegrationTests()
        {
            _fileSystem = CreateFileSystemMock(_settings);
            var config = new ServerConfig(_settings, NullLogger<ServerConfig>.Instance, _fileSystem);
            _serviceProvider = CreateServiceProvider(_settings, config, _fileSystem);

            _keysDirectory = MockUnixSupport.Path($"{_settings.ServerDirectory}\\{KeysConstants.KeysDirectoryName}");
            _externalKeysDirectory = MockUnixSupport.Path($"{_settings.ServerConfigDirectory}\\{KeysConstants.ExternalKeysDirectoryName}");
            _modsDirectory = _settings.ModsDirectory!;
        }
        
        [Fact, Trait("Category", "Integration")]
        public void PrepareKeysForModset_MultipleModTypesAllWithBikeys_CorrectBikeysCopied()
        {
            var modset = ModsetHelpers.CreateTestModset(_fixture, _modsDirectory);
            foreach (var mod in modset.Mods)
            {
                CreateDirectoryForMod(_fileSystem, mod);
            }
            
            var keysPreparer = _serviceProvider.GetService<IKeysPreparer>()!;

            var result = keysPreparer.PrepareKeysForModset(modset);

            using (new AssertionScope())
            {
                result.ShouldBeSuccess();
                
                var expectedBikeyNames = GetBikeysForMods(_fileSystem, modset.ClientLoadableMods)
                    .Append(new BikeyFile(KeysConstants.ArmaKey))
                    .Select(x => x.FileName)
                    .ToList();

                AssertCorrectBikeysInDirectory(
                    _fileSystem,
                    _keysDirectory,
                    expectedBikeyNames);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public void PrepareKeysForModset_SingleModWithoutKeyWithExternalKey_ExternalBikeyCopied()
        {
            var mod = ModHelpers.CreateTestMod(
                _fixture,
                ModType.Required,
                _modsDirectory);
            CreateDirectoryForMod(_fileSystem, mod, createBikey: false);
            
            var modset = ModsetHelpers.CreateModsetWithMods(_fixture, mod.AsList());
            CreateExternalKeyForMod(_fileSystem, _externalKeysDirectory, mod);

            var keysPreparer = _serviceProvider.GetService<IKeysPreparer>()!;

            var result = keysPreparer.PrepareKeysForModset(modset);

            using (new AssertionScope())
            {
                result.ShouldBeSuccess();
                
                var expectedBikeyNames = new List<string>
                {
                    KeysConstants.ArmaKey,
                    $"{mod.Name}{KeysConstants.KeyExtension}"
                };

                AssertCorrectBikeysInDirectory(
                    _fileSystem,
                    _keysDirectory,
                    expectedBikeyNames);
            }
        }
        
        [Fact, Trait("Category", "Integration")]
        public void PrepareKeysForModset_NoModsArmaBikeyOnly_RemovesOldKeysFromKeysDirectory()
        {
            var modset = ModsetHelpers.CreateEmptyModset(_fixture);
            _fileSystem.CreateBikeyFileInFileSystem(_keysDirectory, _fixture.CreateFileName(KeysConstants.KeyExtension));

            var keysPreparer = _serviceProvider.GetService<IKeysPreparer>()!;

            var result = keysPreparer.PrepareKeysForModset(modset);

            using (new AssertionScope())
            {
                result.ShouldBeSuccess();

                var expectedBikeyNames = KeysConstants.ArmaKey.AsList();

                AssertCorrectBikeysInDirectory(
                    _fileSystem,
                    _keysDirectory,
                    expectedBikeyNames);
            }
        }

        private void AssertCorrectBikeysInDirectory(
            IFileSystem fileSystem,
            string keysDirectory,
            IReadOnlyCollection<string>? expectedBikeyNames)
        {
            var copiedBikeys = GetCopiedBikeys(fileSystem, keysDirectory)
                .Select(x => x.FileName)
                .ToList();

            copiedBikeys.Should().BeEquivalentTo(expectedBikeyNames);
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
                    KeysConstants.KeyExtensionSearchPattern,
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
                    _fixture.CreateFileName(KeysConstants.KeyExtension));
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
