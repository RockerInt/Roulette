using RouletteApi.Models.Enums;
using System;
using System.Collections.Generic;

namespace RouletteApi.Models
{
    [Serializable]
    public abstract class RouletteBase
    {
        public RouletteBase()
        {
            Bets = new List<BetUser>();
        }

        public Guid Id { get; set; }
        public List<BetUser> Bets { get; set; }
    }
}
