using System;

namespace RouletteApi.Models
{
    public class UserBet
    {
        public Guid UserId { get; set; }
        public Bet Bet { get; set; }
    }
}