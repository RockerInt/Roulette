using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RouletteApi.Infrastructure
{
    public class RedisConfig
    {
        public bool AllowAdmin { get; set; }
        public List<(string host, int port)> EndPoints { get; set; }
    }
}
