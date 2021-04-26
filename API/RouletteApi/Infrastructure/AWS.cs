using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.AwsCloudWatch;
using System;
using System.Globalization;

namespace RouletteApi.Infrastructure
{
    public static class AWS
    {
        private static readonly string ENVIRONMENT_VARIABLE_ACCESSKEY = "AWS_ACCESS_KEY_ID";
        private static readonly string ENVIRONMENT_VARIABLE_SECRETKEY = "AWS_SECRET_ACCESS_KEY";
        private static readonly string ENVIRONMENT_VARIABLE_SESSION_TOKEN = "AWS_SESSION_TOKEN";
        private static ImmutableCredentials _credentials;

        public static ImmutableCredentials Credentials { get => _credentials ?? GetCredentials(); }

        public static (CloudWatchSinkOptions options, AmazonCloudWatchLogsClient client) GetCloudWatchLogsOptions(IConfiguration configuration)
        {
            //int.TryParse(configuration.GetSection("Serilog").GetSection("LogLevel").Value, out int level);
            int.TryParse(configuration.GetSection("Serilog").GetSection("RententionPolicy").Value, out int retentionConfig);
            //var logLevel = typeof(LogEventLevel).IsEnumDefined(level) ?
            //                    (LogEventLevel)level : LogEventLevel.Error;
            var retentionPolicy = typeof(LogGroupRetentionPolicy).IsEnumDefined(retentionConfig) ?
                              (LogGroupRetentionPolicy)retentionConfig : LogGroupRetentionPolicy.OneWeek;
            var region = RegionEndpoint.GetBySystemName(configuration.GetSection("Serilog").GetSection("Region").Value);
            //var levelSwitch = new LoggingLevelSwitch();
            //levelSwitch.MinimumLevel = logLevel;
            var options = new CloudWatchSinkOptions
            {
                TextFormatter = new JsonTextFormatter(),
                LogGroupName = configuration.GetSection("Serilog").GetSection("LogGroup").Value,
                //MinimumLogEventLevel = logLevel,
                BatchSizeLimit = 100,
                QueueSizeLimit = 10000,
                Period = TimeSpan.FromSeconds(10),
                CreateLogGroup = true,
                LogStreamNameProvider = new DefaultLogStreamProvider(),
                RetryAttempts = 5,
                LogGroupRetentionPolicy = retentionPolicy
            };
            var client = new AmazonCloudWatchLogsClient(Credentials.AccessKey, Credentials.SecretKey, Credentials.Token, region);

            return (options, client);
        }

        private static ImmutableCredentials GetCredentials()
        {
            string accessKeyId = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE_ACCESSKEY);
            string secretKey = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE_SECRETKEY);
            string sessionToken = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE_SESSION_TOKEN);
            if (string.IsNullOrEmpty(accessKeyId) || string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "The environment variables {0}/{1}/{2} were not set with AWS credentials.",
                    ENVIRONMENT_VARIABLE_ACCESSKEY, ENVIRONMENT_VARIABLE_SECRETKEY, ENVIRONMENT_VARIABLE_SESSION_TOKEN));
            }
            _credentials = new ImmutableCredentials(accessKeyId, secretKey, sessionToken);

            return _credentials;
        }
    }
}
