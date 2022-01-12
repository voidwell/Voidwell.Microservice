using IdentityModel;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Linq;

namespace Voidwell.Microservice.Logging.Enrichers
{
    public class ClaimsPrincipalEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsPrincipalEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (_httpContextAccessor?.HttpContext?.User == null )
            {
                return;
            }

            GetProperties()
                .Select(a => propertyFactory.CreateProperty(a.Key, a.Value))
                .ToList()
                .ForEach(logEvent.AddPropertyIfAbsent);
        }

        private IEnumerable<KeyValuePair<string, string>> GetProperties()
        {
            if (TryGetUserProperty(JwtClaimTypes.Subject, out var userClaim))
            {
                yield return new KeyValuePair<string, string>("User", userClaim);
            }

            if (TryGetUserProperty(JwtClaimTypes.ClientId, out var clientClaim))
            {
                yield return new KeyValuePair<string, string>("Client", clientClaim);
            }

            if (TryGetUserProperty(JwtClaimTypes.SessionId, out var sessionClaim))
            {
                yield return new KeyValuePair<string, string>("Session", sessionClaim);
            }
        }

        private bool TryGetUserProperty(string type, out string value)
        {
            value = string.Empty;
            var claim = _httpContextAccessor.HttpContext.User.FindFirst(type);
            if (claim != null)
            {
                value = claim.Value;
                return true;
            }
            return false;
        }
    }
}
