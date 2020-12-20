using System;
using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Clients.Modsets.Entities;
using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Clients.Modsets.Entities {
    public class WebModDeserializationTests {
        const string ApiDateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";
        private readonly Fixture _fixture = new Fixture();
        private string _modId;
        private string _modName;
        private DateTime _modCreatedAt;
        private DateTime _modLastUpdatedAt;
        private WebModSource _modSource;
        private WebModType _modType;
        private int _workshopItemId;
        private string _directory;

        private Dictionary<string, object> _jsonDictionary;


        [Fact]
        public void Mod_Deserialize_Successfull() {
            _jsonDictionary = PrepareModDictionary();
            var json = JsonConvert.SerializeObject(_jsonDictionary);

            var mod = JsonConvert.DeserializeObject<WebMod>(json);

            mod.Id.Should().Be(_modId);
            mod.Name.Should().Be(_modName);
            mod.CreatedAt.GetType().Should().Be<DateTime>();
            mod.CreatedAt.Should().BeCloseTo(_modCreatedAt, TimeSpan.FromSeconds(1));
            mod.LastUpdatedAt.GetType().Should().Be<DateTime>();
            mod.LastUpdatedAt.Should().BeCloseTo(_modLastUpdatedAt, TimeSpan.FromSeconds(1));
            mod.Source.GetType().Should().Be<WebModSource>();
            mod.Source.Should().Be(_modSource);
            mod.Type.GetType().Should().Be<WebModType>();
            mod.Type.Should().Be(_modType);
            mod.ItemId.Should().Be(_workshopItemId);
            mod.Directory.Should().Be(_directory);
        }
        
        private Dictionary<string, object> PrepareModDictionary() {
            _modId = _fixture.Create<string>();
            _modName = _fixture.Create<string>();
            _modCreatedAt = _fixture.Create<DateTime>();
            _modLastUpdatedAt = _fixture.Create<DateTime>();
            _modSource = _fixture.Create<WebModSource>();
            _modType = _fixture.Create<WebModType>();
            _workshopItemId = _fixture.Create<int>();
            _directory = null;
            _jsonDictionary = new Dictionary<string, object>();
            _jsonDictionary.Add("id", _modId);
            _jsonDictionary.Add("name", _modName);
            _jsonDictionary.Add("createdAt", _modCreatedAt.ToString(ApiDateTimeFormat));
            _jsonDictionary.Add("lastUpdatedAt", _modLastUpdatedAt.ToString(ApiDateTimeFormat));
            _jsonDictionary.Add("source", _modSource.ToString());
            _jsonDictionary.Add("type", _modType.ToString());
            _jsonDictionary.Add("itemId", _workshopItemId);
            _jsonDictionary.Add("directory", _directory);
            return _jsonDictionary;
        }
    }
}