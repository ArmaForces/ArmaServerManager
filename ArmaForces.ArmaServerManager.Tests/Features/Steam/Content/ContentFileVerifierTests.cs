using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using ArmaForces.ArmaServerManager.Features.Steam.Content;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Steam.Content
{
    public class ContentFileVerifierTests
    {
        private readonly string _baseDirectory = Directory.GetCurrentDirectory();

        [Theory, Trait("Category", "Unit")]
        [ClassData(typeof(ContentFileVerifierTestsDataProvider))]
        public void RemoveRedundantFiles(IReadOnlyCollection<string> filesPresent, IReadOnlyCollection<string> filesExpected)
        {
            var fileSystemMock = new MockFileSystem(CreateTestFileSystem(filesPresent), _baseDirectory);
            var contentFileVerifier = new ContentFileVerifier(new NullLogger<ContentFileVerifier>(), fileSystemMock);

            contentFileVerifier.RemoveRedundantFiles(_baseDirectory, filesExpected);

            var filesExpectedToBeRemoved = filesPresent
                .Where(s => !filesExpected.Contains(s));

            using (new AssertionScope())
            {
                foreach (var fileExpected in filesExpected)
                {
                    if (Path.HasExtension(fileExpected))
                    {
                        fileSystemMock.File.Exists(fileExpected).Should().BeTrue();
                    }
                    else
                    {
                        fileSystemMock.Directory.Exists(fileExpected).Should().BeTrue();
                    }
                }

                foreach (var fileExpectedToBeRemoved in filesExpectedToBeRemoved)
                {
                    if (Path.HasExtension(fileExpectedToBeRemoved))
                    {
                        fileSystemMock.File.Exists(fileExpectedToBeRemoved).Should().BeFalse();
                    }
                    else
                    {
                        fileSystemMock.Directory.Exists(fileExpectedToBeRemoved).Should().BeFalse();
                    }
                }
            }
        }

        private static Dictionary<string, MockFileData> CreateTestFileSystem(IEnumerable<string> filesPresent)
        {
            return filesPresent
                .Select(
                    x => Path.HasExtension(x)
                        ? new KeyValuePair<string, MockFileData>(x, new MockFileData(string.Empty))
                        : new KeyValuePair<string, MockFileData>(x, new MockDirectoryData()))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        internal class ContentFileVerifierTestsDataProvider : IEnumerable<object[]>
        {
            private readonly IEnumerable<object[]> _enumerableImplementation;

            public ContentFileVerifierTestsDataProvider()
            {
                _enumerableImplementation = new List<object[]>
                {
                    SimpleTestData()
                };
            }

            public IEnumerator<object[]> GetEnumerator() => _enumerableImplementation.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _enumerableImplementation).GetEnumerator();

            private object[] SimpleTestData()
            {
                return new object[]
                {
                    ModBaseFilesAndDirectories
                        .Concat(MainPbo)
                        .Concat(Test2Pbo)
                        .Concat(TestBikey)
                        .Concat(Test2Bikey)
                        .ToList(),
                    ModBaseFilesAndDirectories
                        .Concat(MainPbo)
                        .Concat(TestBikey)
                        .ToList()
                };
            }

            private List<string> ModBaseFilesAndDirectories => new List<string>
            {
                AddonsDirectory,
                KeysDirectory,
                ConfigCpp
            };

            private string AddonsDirectory => "Addons";

            private string KeysDirectory => "Keys";

            private string ConfigCpp => "config.cpp";

            private List<string> MainPbo => new List<string>
            {
                Path.Join(AddonsDirectory, "main.pbo"),
                Path.Join(AddonsDirectory, "main.pbo.bisign")
            };

            private List<string> Test2Pbo => new List<string>
            {
                Path.Join(AddonsDirectory, "test2.pbo"),
                Path.Join(AddonsDirectory, "test2.pbo.bisign")
            };

            private List<string> TestBikey => new List<string>
            {
                Path.Join(KeysDirectory, "test.bikey")
            };

            private List<string> Test2Bikey => new List<string>
            {
                Path.Join(KeysDirectory, "test2.bikey")
            };
        }
    }
}
