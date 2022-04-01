

using System;
using System.Net.Http;

namespace Voidwell.Microservice.Authentication
{
    public class ServiceAuthenticationOptions
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public SupportedTokens SupportedTokens { get; set; }
        public bool SaveToken { get; set; }
        public TimeSpan CacheDuration { get; set; }
        public string RoleClaimType { get; set; }
        public string NameClaimType { get; set; }
        public bool RequireHttpsMetadata { get; set; }
        public bool EnableCaching { get; set; }
        public string Audience { get; set; }
        public HttpMessageHandler BackchannelHttpHandler { get; set; }
    }


}
