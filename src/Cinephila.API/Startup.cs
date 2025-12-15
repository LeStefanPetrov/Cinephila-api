using Cinephila.API.Middleware;
using Cinephila.API.StartupExtensions;
using Google.Apis.Oauth2.v2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cinephila.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDatabases(Configuration);
            services.LoadConfigurations(Configuration);
            services.LoadSerializationOptions();
            services.AddAuthentication(Configuration);
            services.AddControllers();
            services.AddMappingProfiles();
            services.AddRepositories();
            services.AddServices();
            services.AddBackgroundServices();
            services.AddHttpClients(Configuration);
            services.AddValidators();
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cinephila.API v1");
                    c.DocumentTitle = $"Cinephila Service";
                    c.OAuthClientId("21758989588-o99527rg1tidhva82aigfg1u6ku81b6q.apps.googleusercontent.com");
                    c.OAuthClientSecret("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");
                    c.OAuthScopes([Oauth2Service.Scope.UserinfoProfile, Oauth2Service.Scope.UserinfoEmail]);
                    c.EnableDeepLinking();
                });
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseRouting();

            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin();
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
            });

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
