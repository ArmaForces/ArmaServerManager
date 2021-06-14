using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using ArmaForces.Arma.Server.Features.Keys.Finder;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using ArmaForces.Arma.Server.Features.Keys.Models;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Features.Keys.Finder
{
    public class KeysFinderUnitTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly string _workingDirectory;

        public KeysFinderUnitTests()
        {
            _workingDirectory = _fixture.Create<string>();
        }
        
        [Fact, Trait("Category", "Unit")]
        public void GetKeysFromDirectory_NoKeys_ReturnsEmptyList()
        {
            var mockedFileSystem = CreateMockedFileSystem(_workingDirectory);
            CreateRandomFileInDirectory(mockedFileSystem, _workingDirectory);

            var keysFinder = CreateKeysFinder(mockedFileSystem);

            var keysFromDirectory = keysFinder.GetKeysFromDirectory(_workingDirectory);

            keysFromDirectory.Should().BeEmpty();
        }
        
        [Fact, Trait("Category", "Unit")]
        public void GetKeysFromDirectory_OneKeyIncluded_ReturnsListWithOneKey()
        {
            var mockedFileSystem = CreateMockedFileSystem(_workingDirectory);
            CreateRandomFileInDirectory(mockedFileSystem, _workingDirectory);
            
            var bikeyFiles = CreateBikeyFilesInFileSystem(mockedFileSystem, _workingDirectory);

            var keysFinder = CreateKeysFinder(mockedFileSystem);

            var keysFromDirectory = keysFinder.GetKeysFromDirectory(_workingDirectory);

            using (new AssertionScope())
            {
                keysFromDirectory.Should().HaveCount(1);
                keysFromDirectory.Should().BeEquivalentTo(bikeyFiles);
            }
        }
        
        [Fact, Trait("Category", "Unit")]
        public void GetKeysFromDirectory_MultipleKeysInSubfolders_ReturnsAllKeys()
        {
            var mockedFileSystem = CreateMockedFileSystem(_workingDirectory);
            CreateRandomFileInDirectory(mockedFileSystem, _workingDirectory);
            
            var subDirectory = mockedFileSystem.Path.Join(_workingDirectory, _fixture.Create<string>());
            var expectedBikeyFiles = CreateBikeyFilesInFileSystem(
                mockedFileSystem,
                subDirectory,
                count: 5);

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
                .Select(fileName => CreateBikeyFileInFileSystem(fileSystem, directory, fileName))
                .ToList();
        }

        private static BikeyFile CreateBikeyFileInFileSystem(
            IFileSystem fileSystem,
            string directory,
            string fileName,
            string fileExtension = "bikey")
        {
            var drive = MockUnixSupport.Path("C:\\");
            var fileNameWithExtension = $"{fileName}.{fileExtension}";
            var filePath = fileSystem.Path.Join(drive, directory, fileNameWithExtension);

            var bikeyFile = new BikeyFile(filePath);
            CreateFileInFileSystem(fileSystem, filePath);

            return bikeyFile;
        }

        private void CreateRandomFileInDirectory(IFileSystem fileSystem, string directory)
        {
            var filePath = Path.Join(directory, _fixture.Create<string>());
            CreateFileInFileSystem(fileSystem, filePath);
        }

        private static void CreateFileInFileSystem(IFileSystem fileSystem, string filePath)
        {
            var separator = MockUnixSupport.Path("\\");
            
            var directory = filePath
                .Split(separator)
                .SkipLast(1)
                .Aggregate((x, y) => $"{x}{separator}{y}");
            
            if (!fileSystem.Directory.Exists(directory))
            {
                fileSystem.Directory.CreateDirectory(directory);
            }
            
            fileSystem.File.Create(filePath);
        }

        private static IKeysFinder CreateKeysFinder(IFileSystem fileSystem)
        {
            var logger = new NullLogger<KeysFinder>();
            return new KeysFinder(logger, fileSystem);
        }
    }
}
