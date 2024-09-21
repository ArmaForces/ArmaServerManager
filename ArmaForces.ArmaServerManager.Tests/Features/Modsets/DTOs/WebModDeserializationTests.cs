using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArmaForces.Arma.Server.Constants;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Modsets.DTOs
{
    [Trait("Category", "Unit")]
    public class WebModDeserializationTests
    {
        const string ApiDateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";
        private readonly Fixture _fixture = new Fixture();
        private string? _modId;
        private string? _modName;
        private DateTime _modCreatedAt;
        private DateTime _modLastUpdatedAt;
        private WebModSource _modSource;
        private WebModType _modType;
        private int _workshopItemId;
        private string? _directory;
        
        [Fact]
        public void Mod_Deserialize_Successfull()
        {
            var jsonDictionary = PrepareModDictionary();
            var json = JsonSerializer.Serialize(jsonDictionary);

            var mod = JsonSerializer.Deserialize<WebMod>(json, JsonOptions.Default);
            mod.Should().NotBeNull();

            mod!.Id.Should().Be(_modId);
            mod.Name.Should().Be(_modName);
            mod.CreatedAt.GetType().Should().Be<DateTime>();
            mod.CreatedAt.Should().BeCloseTo(_modCreatedAt, TimeSpan.FromSeconds(1));
            mod.LastUpdatedAt!.GetType().Should().Be<DateTime>();
            mod.LastUpdatedAt.Should().BeCloseTo(_modLastUpdatedAt, TimeSpan.FromSeconds(1));
            mod.Source.GetType().Should().Be<WebModSource>();
            mod.Source.Should().Be(_modSource);
            mod.Type.GetType().Should().Be<WebModType>();
            mod.Type.Should().Be(_modType);
            mod.ItemId.Should().Be(_workshopItemId);
            mod.Directory.Should().Be(_directory);
        }
        
        private Dictionary<string, object> PrepareModDictionary()
        {
            _modId = _fixture.Create<string>();
            _modName = _fixture.Create<string>();
            _modCreatedAt = _fixture.Create<DateTime>();
            _modLastUpdatedAt = _fixture.Create<DateTime>();
            _modSource = _fixture.Create<WebModSource>();
            _modType = _fixture.Create<WebModType>();
            _workshopItemId = _fixture.Create<int>();
            _directory = null;

            return new Dictionary<string, object>
            {
                {"id", _modId},
                {"name", _modName},
                {"createdAt", _modCreatedAt.ToString(ApiDateTimeFormat)},
                {"lastUpdatedAt", _modLastUpdatedAt.ToString(ApiDateTimeFormat)},
                {"source", ConvertEnumToString(_modSource)},
                {"type", ConvertEnumToString(_modType)},
                {"itemId", _workshopItemId},
                {"directory", _directory!}
            };
        }

        private static string ConvertEnumToString<T>(T enumValue) where T : Enum
        {
            var enumType = typeof(T);
            var enumName = Enum.GetName(enumType, enumValue) ?? enumValue.ToString();
            var customName = enumType.GetField(enumName)?
                .GetCustomAttributes(typeof(JsonStringEnumMemberNameAttribute), true)
                .SingleOrDefault()?
                .As<JsonStringEnumMemberNameAttribute>()?
                .Name;
            
            return customName ?? enumName;
        }
    }
}