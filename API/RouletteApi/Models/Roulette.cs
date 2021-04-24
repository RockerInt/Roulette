using System;
using System.Collections.Generic;

namespace RouletteApi.Models
{
    [Serializable]
    public class Roulette : RouletteBase
    {
        public Roulette()
        {
            Bets = new List<BetUser>();
        }

        public bool IsOpen { get; set; } = false;
    }
}
