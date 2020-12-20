using System;
using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Clients.Modsets.Entities;
using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Clients.Modsets.Entities {
    public class WebModsetDeserializationTests {

        const string ApiDateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";
        private readonly Fixture _fixture = new Fixture();
        private readonly string _modsetId;
        private readonly string _modsetName;
        private readonly DateTime _modsetCreatedAt;
        private readonly DateTime _modsetLastUpdatedAt;

        private Dictionary<string, object> _jsonDictionary;

        public WebModsetDeserializationTests() {
            _modsetId = _fixture.Create<string>();
            _modsetName = _fixture.Create<string>();
            _modsetCreatedAt = _fixture.Create<DateTime>();
            _modsetLastUpdatedAt = _fixture.Create<DateTime>();
            _jsonDictionary = new Dictionary<string, object>();
            _jsonDictionary.Add("id", _modsetId);
            _jsonDictionary.Add("name", _modsetName);
            _jsonDictionary.Add("createdAt", _modsetCreatedAt.ToString(ApiDateTimeFormat));
            _jsonDictionary.Add("lastUpdatedAt", _modsetLastUpdatedAt.ToString(ApiDateTimeFormat));
        }

        [Fact]
        public void Modset_DeserializeWithoutMods_Successfull() {
            var json = JsonConvert.SerializeObject(_jsonDictionary);

            var modset = JsonConvert.DeserializeObject<WebModset>(json);

            modset.Id.Should().Be(_modsetId);
            modset.Name.Should().Be(_modsetName);
            modset.CreatedAt.GetType().Should().Be<DateTime>();
            modset.CreatedAt.Should().BeCloseTo(_modsetCreatedAt, TimeSpan.FromSeconds(1));
            modset.LastUpdatedAt.GetType().Should().Be<DateTime>();
            modset.LastUpdatedAt.Should().BeCloseTo(_modsetLastUpdatedAt, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Modset_DeserializeWithMods_Successfull() {
            var modsList = CreateModsList();
            _jsonDictionary.Add("mods", modsList);
            var json = JsonConvert.SerializeObject(_jsonDictionary);

            var modset = JsonConvert.DeserializeObject<WebModset>(json);

            modset.Mods.GetType().Should().Be<List<WebMod>>();
            modset.Mods.Count.Should().Be(1);
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