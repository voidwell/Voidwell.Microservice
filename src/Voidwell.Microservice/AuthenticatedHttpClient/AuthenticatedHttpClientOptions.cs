using System;
using System.Collections.Generic;

namespace Voidwell.Microservice.AuthenticatedHttpClient
{
    public class AuthenticatedHttpClientOptions
    {
        public AuthenticatedHttpClientOptions()
        {
            Scopes = new List<string>();
        }

        public string TokenServiceAddress { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public List<string> Scopes { get; set; }
    }
}
