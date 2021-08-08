using System.Collections.Generic;
using System.Linq;
using ArmaForces.Arma.Server.Tests.Helpers;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using AutoFixture;

namespace ArmaForces.ArmaServerManager.Tests.Features.Modsets.TestingHelpers
{
    public class ModsetsStorage
    {
        private readonly Fixture _fixture = new Fixture();
        
        public IReadOnlyList<WebModset> Modsets { get; }

        public ModsetsStorage()
        {
            Modsets = CreateModsets().ToList();
        }

        private IEnumerable<WebModset> CreateModsets(int count = 10)
        {
            for (var i = 0; i < count; i++)
            {
                yield return WebModsetHelper.CreateTestModset(_fixture);
            }
        }
    }
}
