using System;

namespace RouletteApi.Models
{
    [Serializable]
    public class BetUser
    {
        public Guid UserId { get; set; }
        public decimal BetResult { get; set; }
        public Bet Bet { get; set; }
    }
}