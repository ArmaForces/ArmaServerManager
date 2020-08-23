using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace Arma.Server.Test.Modlist {
    public class DeserializationTests {

        const string ApiDateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";
        private readonly Fixture _fixture = new Fixture();
        private readonly string _modlistId;
        private readonly string _modlistName;
        private readonly DateTime _modlistCreatedAt;
        private readonly DateTime _modlistLastUpdatedAt;

        private Dictionary<string, object> _jsonDictionary;

        public DeserializationTests() {
            _modlistId = _fixture.Create<string>();
            _modlistName = _fixture.Create<string>();
            _modlistCreatedAt = _fixture.Create<DateTime>();
            _modlistLastUpdatedAt = _fixture.Create<DateTime>();
            _jsonDictionary = new Dictionary<string, object>();
            _jsonDictionary.Add("id", _modlistId);
            _jsonDictionary.Add("name", _modlistName);
            _jsonDictionary.Add("createdAt", _modlistCreatedAt.ToString(ApiDateTimeFormat));
            _jsonDictionary.Add("lastUpdatedAt", _modlistLastUpdatedAt.ToString(ApiDateTimeFormat));
        }

        [Fact]
        public void Modlist_DeserializeWithoutMods_Successfull() {
            var json = JsonConvert.SerializeObject(_jsonDictionary);

            var modlist = JsonConvert.DeserializeObject<Arma.Server.Modlist.Modlist>(json);

            modlist.Id.Should().Be(_modlistId);
            modlist.Name.Should().Be(_modlistName);
            modlist.CreatedAt.GetType().Should().Be<DateTime>();
            modlist.CreatedAt.Should().BeCloseTo(_modlistCreatedAt, TimeSpan.FromSeconds(1));
            modlist.LastUpdatedAt.GetType().Should().Be<DateTime>();
            modlist.LastUpdatedAt.Should().BeCloseTo(_modlistLastUpdatedAt, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Modlist_DeserializeWithMods_Successfull() {
            var modsList = CreateModsList();
            _jsonDictionary.Add("mods", modsList);
            var json = JsonConvert.SerializeObject(_jsonDictionary);

            var modlist = JsonConvert.DeserializeObject<Arma.Server.Modlist.Modlist>(json);

            modlist.Mods.GetType().Should().Be<List<Arma.Server.Mod.Mod>>();
            modlist.Mods.Count.Should().Be(1);
        }

        private List<object> CreateModsList() {
            List<object> modsList = new List<object>();
            modsList.Add(CreateModDictionary());
            return modsList;
        }
        
        private Dictionary<string, object> CreateModDictionary() {
            Dictionary<string, object> mod = new Dictionary<string, object>();
            return mod;
        }
    }
}