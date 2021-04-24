using System;
using System.Collections.Generic;

namespace RouletteApi.Models
{
    [Serializable]
    public class Roulette
    {
        public Roulette()
        {
            Bets = new List<UserBet>();
        }

        public Guid Key { get; set; }
        public bool OpenBets { get; set; } = false;
        public IEnumerable<UserBet> Bets { get; set; }
    }
}
