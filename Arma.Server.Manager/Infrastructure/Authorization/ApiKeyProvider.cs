﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Arma.Server.Manager.Infrastructure.Authorization
{
    public class ApiKeyProvider : IApiKeyProvider
    {
        private const string ConfigurationSectionName = "ApiKeys";
        
        private readonly IConfiguration _configuration;

        public ApiKeyProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<string> GetAcceptedApiKeys()
        {
            return _configuration
                .GetSection(ConfigurationSectionName)
                .GetChildren()
                .Select(x => x.Value);
        }
    }

    public interface IApiKeyProvider
    {
        IEnumerable<string> GetAcceptedApiKeys();
    }
}
