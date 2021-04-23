using StackExchange.Redis;
using System;

namespace RouletteApi.Persistence
{
    public interface ICacheClient
    {
        bool HasValue(RedisKey key);
        T JsonGet<T>(RedisKey key, CommandFlags flags = CommandFlags.None);
        bool JsonSet(RedisKey key, object value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None);
    }
}