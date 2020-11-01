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
                var content = File.ReadAllText(filePath);
                var fileName = Path.GetFileName(filePath);
                var mockedFilePath = Path.Join(baseDirectory, fileName);
                fileSystemMock.File.WriteAllText(mockedFilePath, content);
            }
        }
    }
}
