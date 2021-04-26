using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RouletteApi.Infrastructure;
using RouletteApi.Persistence;
using RouletteApi.Repositories;

namespace RouletteApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin();
                });
            });
            services.AddOptions();
            services.Configure<RedisConfig>(Configuration.GetSection("Redis"));
            services.AddScoped<ICacheClient, CacheClient>();
            services.AddScoped<IRouletteRepository, RouletteRepository>();
            services.AddSwagger()
                    .AddControllers();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RouletteApi"));
            }
            app.UseRouting();
            app.UseCors();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public static class CustomExtensionMethods
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services) =>
            services.AddSwaggerGen(options =>
            {
                var version = "v1";
                options.SwaggerDoc(version, new OpenApiInfo
                {
                    Title = $"Roulette API - {version}",
                    Version = version,
                    Description = "The Roulette API"
                });
            });
    }
}
