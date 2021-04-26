using System;

namespace RouletteApi.Models
{
    public class MakeBet
    {
        public Guid RouletteId { get; set; }
        public Bet Bet { get; set; }
    }
}