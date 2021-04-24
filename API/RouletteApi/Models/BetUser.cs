using System;

namespace RouletteApi.Models
{
    public class BetUser
    {
        public Guid UserId { get; set; }
        public decimal BetResult { get; set; }
        public Bet Bet { get; set; }
    }
}