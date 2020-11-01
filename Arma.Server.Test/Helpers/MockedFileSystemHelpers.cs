using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace Arma.Server.Test.Helpers
{
    public class MockedFileSystemHelpers
    {
        public static void CopyExampleFilesToMockedFileSystem(IFileSystem fileSystemMock, string baseDirectory = null)
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

        public static void CopyTestFilesToMockedFileSystem(IFileSystem fileSystemMock, string baseDirectory = null)
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
            string mockedFilePath = null,
            string baseDirectory = null)
        {
            baseDirectory ??= fileSystemMock.Directory.GetCurrentDirectory();

            var content = File.ReadAllText(filePath);
            var fileName = Path.GetFileName(filePath);
            mockedFilePath ??= Path.Join(baseDirectory, fileName);
            fileSystemMock.File.WriteAllText(mockedFilePath, content);
        }
    }
}
