using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.AwsCloudWatch;
using System;
using System.IO;
using System.Linq;

namespace RouletteApi
{
    public class Program
    {
        private const string AppName = "RouletteApi";
        public static int Main(string[] args)
        {
            Log.Logger = CreateSerilogLogger(GetConfiguration());
            try
            {
                Log.Information("Configuring web host ({ApplicationContext})...", AppName);
                var host = CreateHostBuilder(args);

                Log.Information("Starting web host ({ApplicationContext})...", AppName);
                host.Build().Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", AppName);
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration(config =>
                {
                    config = EnvironmentSettings(config);
                    if (args != null)
                        config.AddCommandLine(args);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static IConfigurationBuilder EnvironmentSettings(IConfigurationBuilder config)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var envFile = Environment.GetEnvironmentVariable("ENV_FILE") ?? env;
            config.Sources.Remove(
                config.Sources.FirstOrDefault(x => x.GetType().Name == "JsonConfigurationSource"
                    && ((Microsoft.Extensions.Configuration.Json.JsonConfigurationSource)x).Path == $"appsettings.{env}.json"));

            config.AddJsonFile($"appsettings.{envFile}.json", optional: true, reloadOnChange: true);
            return config.AddEnvironmentVariables();
        }

        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder = EnvironmentSettings(builder);

            return builder.Build();
        }

        private static ILogger CreateSerilogLogger(IConfiguration configuration)
        {
            var mongoServerUrl = configuration["Serilog:MongoServerUrl"];
            var (options, client) = Infrastructure.AWS.GetCloudWatchLogsOptions(configuration);

            var config = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("ApplicationContext", "RouletteApi")
                .Enrich.FromLogContext()
                .WriteTo.Console();

            if (!string.IsNullOrWhiteSpace(mongoServerUrl))
                config = config.WriteTo.MongoDB(mongoServerUrl, collectionName: "roulette.log");
                
            return config.WriteTo.AmazonCloudWatch(options, client)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

    }
}
