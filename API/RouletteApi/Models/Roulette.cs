using System;
using System.Collections.Generic;

namespace RouletteApi.Models
{
    [Serializable]
    public class Roulette
    {
        public Roulette()
        {
            Bets = new List<BetUser>();
        }

        public Guid Id { get; set; }
        public bool IsOpen { get; set; } = false;
        public List<BetUser> Bets { get; set; }
    }
}
