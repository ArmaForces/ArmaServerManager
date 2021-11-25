using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using ArmaForces.Arma.Server.Features.Keys.Finder;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using ArmaForces.Arma.Server.Features.Keys.Models;
using ArmaForces.Arma.Server.Tests.Helpers;
using ArmaForces.Arma.Server.Tests.Helpers.Extensions;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Features.Keys.Finder
{
    [Trait("Category", "Unit")]
    public class KeysFinderUnitTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly string _workingDirectory;

        public KeysFinderUnitTests()
        {
            _workingDirectory = _fixture.Create<string>();
        }
        
        [Fact]
        public void GetKeysFromDirectory_NoKeys_ReturnsEmptyList()
        {
            var mockedFileSystem = CreateMockedFileSystem(_workingDirectory);
            CreateRandomFileInDirectory(mockedFileSystem, _workingDirectory);

            var keysFinder = CreateKeysFinder(mockedFileSystem);

            var keysFromDirectory = keysFinder.GetKeysFromDirectory(_workingDirectory);

            keysFromDirectory.Should().BeEmpty();
        }
        
        [Fact]
        public void GetKeysFromDirectory_OneKeyIncluded_ReturnsListWithOneKey()
        {
            var mockedFileSystem = CreateMockedFileSystem(_workingDirectory);
            CreateRandomFileInDirectory(mockedFileSystem, _workingDirectory);
            
            var bikeyFiles = CreateBikeyFilesInFileSystem(mockedFileSystem, _workingDirectory)
                .Select(x => x.Path)
                .ToList();

            var keysFinder = CreateKeysFinder(mockedFileSystem);

            var keysFromDirectory = keysFinder.GetKeysFromDirectory(_workingDirectory);

            using (new AssertionScope())
            {
                keysFromDirectory.Should().HaveCount(1);
                keysFromDirectory.Should().BeEquivalentTo(bikeyFiles);
            }
        }
        
        [Fact]
        public void GetKeysFromDirectory_MultipleKeysInSubfolders_ReturnsAllKeys()
        {
            var mockedFileSystem = CreateMockedFileSystem(_workingDirectory);
            CreateRandomFileInDirectory(mockedFileSystem, _workingDirectory);
            
            var subDirectory = mockedFileSystem.Path.Join(_workingDirectory, _fixture.Create<string>());
            var expectedBikeyFiles = CreateBikeyFilesInFileSystem(
                mockedFileSystem,
                subDirectory,
                count: 5)
                .Select(x => x.Path)
                .ToList();

            var keysFinder = CreateKeysFinder(mockedFileSystem);

            var keysFromDirectory = keysFinder.GetKeysFromDirectory(_workingDirectory);

            keysFromDirectory.Should().BeEquivalentTo(expectedBikeyFiles);
        }

        private static IFileSystem CreateMockedFileSystem(params string[] directories)
        {
            var mockFileSystem = new MockFileSystem();
            
            foreach (var directory in directories)
            {
                mockFileSystem.Directory.CreateDirectory(directory);
            }

            return mockFileSystem;
        }

        private List<BikeyFile> CreateBikeyFilesInFileSystem(
            IFileSystem fileSystem,
            string directory,
            int count = 1)
        {
            return _fixture.CreateMany<string>(count)
                .Select(fileName => fileSystem.CreateBikeyFileInFileSystem(directory, fileName))
                .ToList();
        }

        private void CreateRandomFileInDirectory(IFileSystem fileSystem, string directory)
        {
            var filePath = Path.Join(directory, _fixture.CreateFileName());
            fileSystem.CreateFileInFileSystem(filePath);
        }

        private static IKeysFinder CreateKeysFinder(IFileSystem fileSystem)
        {
            var logger = new NullLogger<KeysFinder>();
            return new KeysFinder(logger, fileSystem);
        }
    }
}
