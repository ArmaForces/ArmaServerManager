using System.IO;
using System.IO.Abstractions;
using System.Linq;
using ArmaForces.Arma.Server.Features.Keys.Models;

namespace ArmaForces.Arma.Server.Tests.Helpers
{
    public static class MockedFileSystemHelpers
    {
        public static void CopyExampleFilesToMockedFileSystem(IFileSystem fileSystemMock, string? baseDirectory = null)
        {
            baseDirectory ??= Directory.GetCurrentDirectory();

            var exampleFilesPaths = new[] { "basic.cfg", "common.Arma3Profile", "common.json", "server.cfg" }
                .Select(x => x.Insert(0, "example_"))
                .Select(x => Path.Join(baseDirectory, x));

            foreach (var filePath in exampleFilesPaths)
            {
                CopyFileToMockedFileSystem(fileSystemMock, filePath, baseDirectory:baseDirectory);
            }
        }

        public static void CopyTestFilesToMockedFileSystem(IFileSystem fileSystemMock, string? baseDirectory = null)
        {
            baseDirectory ??= Directory.GetCurrentDirectory();

            var exampleFilesPaths = new[] { "common.json", "server.cfg" }
                .Select(x => x.Insert(0, "test_"))
                .Select(x => Path.Join(baseDirectory, x));

            foreach (var filePath in exampleFilesPaths)
            {
                var mockedFilePath = filePath.Replace("test_", "");
                CopyFileToMockedFileSystem(fileSystemMock, filePath, mockedFilePath, baseDirectory);
            }
        }

        private static void CopyFileToMockedFileSystem(
            IFileSystem fileSystemMock,
            string filePath,
            string? mockedFilePath = null,
            string? baseDirectory = null)
        {
            baseDirectory ??= fileSystemMock.Directory.GetCurrentDirectory();

            var content = File.ReadAllText(filePath);
            var fileName = Path.GetFileName(filePath);
            mockedFilePath ??= Path.Join(baseDirectory, fileName);
            fileSystemMock.File.WriteAllText(mockedFilePath, content);
        }
        
        public static BikeyFile CreateBikeyFileInFileSystem(
            this IFileSystem fileSystem,
            string directory,
            string fileName,
            string fileExtension = "bikey")
        {
            var fileNameWithExtension = fileSystem.Path.HasExtension(fileName) 
                ? fileName
                : $"{fileName}.{fileExtension}";
            var filePath = fileSystem.Path.GetFullPath( 
                fileSystem.Path.Join(directory, fileNameWithExtension));
            
            var bikeyFile = new BikeyFile(filePath);
            fileSystem.CreateFileInFileSystem(filePath);

            return bikeyFile;
        }
        
        public static void CreateFileInFileSystem(this IFileSystem fileSystem, string filePath)
        {
            var separator = fileSystem.Path.DirectorySeparatorChar;
            
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
    }
}
