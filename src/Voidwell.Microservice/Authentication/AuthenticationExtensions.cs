using Microsoft.AspNetCore.Authentication;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Voidwell.Microservice.Authentication
{
    public static class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddServiceAuthentication(this AuthenticationBuilder builder, Action<ServiceAuthenticationOptions> optionsAction)
        {
            return builder.AddServiceAuthentication(ServiceAuthenticationDefaults.AuthenticationScheme, optionsAction);
        }

        public static AuthenticationBuilder AddServiceAuthentication(this AuthenticationBuilder builder, string authenticationScheme, Action<ServiceAuthenticationOptions> optionsAction)
        {
            var options = new ServiceAuthenticationOptions();
            optionsAction.Invoke(options);

            if (options.SupportedTokens != SupportedTokens.Reference)
            {
                builder.AddJwtBearer(authenticationScheme, o =>
                {
                    o.Authority = options.Authority;
                    o.Audience = options.Audience;
                    o.SaveToken = options.SaveToken;
                    o.RequireHttpsMetadata = options.RequireHttpsMetadata;
                    o.BackchannelHttpHandler = options.BackchannelHttpHandler;

                    o.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };

                    if (options.SupportedTokens == SupportedTokens.Both)
                    {
                        o.ForwardDefaultSelector = Selector.ForwardReferenceToken("introspection");
                    }
                });
            }

            if (options.SupportedTokens != SupportedTokens.Jwt)
            {
                builder.AddOAuth2Introspection(options.SupportedTokens == SupportedTokens.Reference ? authenticationScheme : "introspection", o =>
                {
                    o.Authority = options.Authority;
                    o.ClientId = options.ClientId;
                    o.ClientSecret = options.ClientSecret;
                    o.SaveToken = options.SaveToken;
                    o.EnableCaching = options.EnableCaching;
                    o.CacheDuration = options.CacheDuration;
                    o.NameClaimType = options.NameClaimType;
                    o.RoleClaimType = options.RoleClaimType;
                });
            }

            return builder;
        }

    }
}
