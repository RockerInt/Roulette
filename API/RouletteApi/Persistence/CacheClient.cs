using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace RouletteApi.Persistence
{
    public class CacheClient : ICacheClient
    {
        private readonly ConfigurationOptions _configuration;
        private readonly string ENVIRONMENT_VARIABLE_REDIS_PASSWORD = "REDIS_PASSWORD";
        private readonly ILogger<CacheClient> _logger;
        private readonly Lazy<IConnectionMultiplexer> _connection;
        public IConnectionMultiplexer Connection => _connection.Value;
        public IDatabase Database => Connection.GetDatabase();

        public CacheClient(IConfiguration configuration, ILogger<CacheClient> logger)
        {
            _logger = logger;
            string host = "localhost"; int port = 6379;
            List<EndPoint> endPoints = new();
            bool.TryParse(configuration.GetSection("Redis").GetSection("AllowAdmin").Value, out bool allowAdmin);
            configuration.GetSection("Redis").GetSection("EndPoints").Bind(endPoints);
            string redisPassword = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE_REDIS_PASSWORD);
            _configuration = new ConfigurationOptions()
            {
                AllowAdmin = allowAdmin,
                ClientName = "Roulette Cache",
                ReconnectRetryPolicy = new LinearRetry(5000),
                AbortOnConnectFail = false,
            };
            if (endPoints?.Any() ?? false)
                endPoints.ForEach(x => _configuration.EndPoints.Add(x));
            else
                _configuration.EndPoints.Add(host, port);
            if (!string.IsNullOrWhiteSpace(redisPassword)) _configuration.Password = redisPassword;
            _connection = new Lazy<IConnectionMultiplexer>(() =>
            {
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_configuration);
                redis.ConfigurationChanged += (sender, args) => _logger.LogInformation("ConfigurationChanged with endpoint: {@EndPoint}", args.EndPoint);
                redis.ConfigurationChangedBroadcast += (sender, args) => _logger.LogInformation("ConfigurationChangedBroadcast with endpoint: {@EndPoint}", args.EndPoint);
                redis.ConnectionRestored += (sender, args) => _logger.LogInformation("ConnectionRestored with args: {@args}", args.EndPoint);
                redis.HashSlotMoved += (sender, args) => _logger.LogInformation("HashSlotMoved with args: {@args}", args.NewEndPoint);
                redis.ConnectionFailed += (sender, args) => _logger.LogError("ConnectionFailed with args: {@args}", args.Exception.Message);
                redis.ErrorMessage += (sender, args) => _logger.LogError("ErrorMessage with args: {@args}", args.Message);
                redis.InternalError += (sender, args) => _logger.LogError("InternalError with args: {@args}", args.Exception);
                return redis;
            });
        }
        public bool HasValue(RedisKey key) => Database.StringGet(key).HasValue;
        public T JsonGet<T>(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            RedisValue rv = Database.StringGet(key, flags);
            if (!rv.HasValue)
                return default;
            T rgv = JsonConvert.DeserializeObject<T>(rv);
            _logger.LogInformation("Redis JsonGet response: {@response}", rgv);
            return rgv;
        }
        public bool JsonSet(RedisKey key, object value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            if (value is null) return false;
            _logger.LogInformation("Redis JsonSet value: {@value}", value);
            return Database.StringSet(key, JsonConvert.SerializeObject(value), expiry, when, flags);
        }
    }
}
