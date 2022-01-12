using Voidwell.Microservice.AuthenticatedHttpClient;
using Voidwell.Microservice.Configuration;
using Voidwell.Microservice.TestApp.Services;
using Voidwell.Microservice.Tracing;

namespace Voidwell.Microservice.TestApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.ConfigureServiceProperties("Voidwell Microservice App");

            services.AddTracing();

            services.AddAuthenticatedHttpClient<ITestAuthenticatedClient, TestAuthenticatedClient>(options =>
            {
                options.TokenServiceAddress = Configuration.GetValue<string>("TestTokenServiceAddress");
                options.ClientId = Configuration.GetValue<string>("TestClientId");
                options.ClientSecret = Configuration.GetValue<string>("TestClientSecret");
                options.Scopes = Configuration.GetSection("TestClientScopes")?.Get<List<string>>();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseTracing();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
