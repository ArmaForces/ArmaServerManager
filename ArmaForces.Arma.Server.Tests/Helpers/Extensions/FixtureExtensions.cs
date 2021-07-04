using AutoFixture;

namespace ArmaForces.Arma.Server.Tests.Helpers.Extensions
{
    public static class FixtureExtensions
    {
        public static string CreateFileName(this Fixture fixture)
            => CreateFileName(fixture, fixture.Create<string>());

        public static string CreateFileName(this Fixture fixture, string extension)
            => fixture.Create<string>() + (extension.StartsWith('.') ? extension : '.' + extension);
    }
}
