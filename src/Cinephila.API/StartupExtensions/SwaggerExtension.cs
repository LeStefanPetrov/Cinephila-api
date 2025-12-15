using System;
using System.Collections.Generic;
using System.Linq;
using Cinephila.API.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Cinephila.API.StartupExtensions
{
    public static class SwaggerExtension
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services, AuthenticationSettings settings)
        {
            return services.AddSwaggerGen(options =>
            {
                options.DescribeAllParametersInCamelCase();
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Cinephila API",
                    Version = "0.0.1",
                    Contact = new OpenApiContact
                    {
                        Name = settings.ClientId,
                    },
                });
                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Extensions = new Dictionary<string, IOpenApiExtension>
                        { { "x-tokenName", new OpenApiString("id_token") } },
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{settings.Authority.TrimEnd('/')}/o/oauth2/v2/auth"),
                            TokenUrl = new Uri("https://oauth2.googleapis.com/token"),
                            Scopes = settings.Scopes.ToDictionary(k => k, v => v),
                        },
                    },
                });
                options.OperationFilter<SwaggerAuthorizationFilter>();
                options.CustomSchemaIds(type => type.ToString());
            });
        }

        private class SwaggerAuthorizationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var authorizeAttribute = (AuthorizeAttribute)context
                    .ApiDescription
                    .ActionDescriptor
                    .EndpointMetadata
                    .FirstOrDefault(e => e.GetType() == typeof(AuthorizeAttribute));

                if (authorizeAttribute == null || authorizeAttribute.AuthenticationSchemes != JwtBearerDefaults.AuthenticationScheme)
                    return;

                operation.Security ??= new List<OpenApiSecurityRequirement>();
                operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme,
                        },
                    },
                    new string[] { }
                },
            });
            }
        }
    }
}
