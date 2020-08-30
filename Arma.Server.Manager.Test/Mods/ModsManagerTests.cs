using Arma.Server.Manager.Mods;
using Arma.Server.Mod;
using AutoFixture;
using FluentAssertions;
using System;
using System.IO;
using Xunit;

namespace Arma.Server.Manager.Test.Mods {
    public class ModsManagerTests: IDisposable {
        private Fixture _fixture = new Fixture();
        private readonly string _workingDirectory = Directory.GetCurrentDirectory();
        private string _modDirectory;

        [Fact]
        public void ModExists_DirectoryNamedIdExists_ReturnsTrue() {
            var mod = FixtureCreateMod();
            _modDirectory = Path.Join(_workingDirectory, mod.WorkshopId.ToString());
            Directory.CreateDirectory(_modDirectory);

            ModsManager.ModExists(_workingDirectory, mod.WorkshopId).Should().BeTrue();
        }

        [Fact]
        public void ModExists_DirectoryNamedIdNotExists_ReturnsFalse() {
            var mod = FixtureCreateMod();

            ModsManager.ModExists(_workingDirectory, mod.WorkshopId).Should().BeFalse();
        }

        [Fact]
        public void ModExists_DirectoryNamedNameExists_ReturnsTrue() {
            var mod = FixtureCreateMod();
            _modDirectory = Path.Join(_workingDirectory, mod.Name);
            Directory.CreateDirectory(_modDirectory);

            ModsManager.ModExists(_workingDirectory, mod.Name).Should().BeTrue();
        }

        [Fact]
        public void ModExists_DirectoryNamedNameNotExists_ReturnsFalse() {
            var mod = FixtureCreateMod();

            ModsManager.ModExists(_workingDirectory, mod.Name).Should().BeFalse();
        }

        [Fact]
        public void ModExists_DirectoryNamedWithAtExists_ReturnsTrue() {
            var mod = FixtureCreateMod();
            _modDirectory = Path.Join(_workingDirectory, "@", mod.Name.ToString());
            Directory.CreateDirectory(_modDirectory);

            ModsManager.ModExists(_workingDirectory, mod.Name).Should().BeTrue();
        }

        [Fact]
        public void ModExists_DirectoryNamedWithAtNotExists_ReturnsFalse() {
            var mod = FixtureCreateMod();

            ModsManager.ModExists(_workingDirectory, mod.Name).Should().BeFalse();
        }

        private IMod FixtureCreateMod() {
            return _fixture.Create<Mod.Mod>();
        }

        public void Dispose() {
            if (_modDirectory != null) Directory.Delete(_modDirectory, true);
        }
    }
}